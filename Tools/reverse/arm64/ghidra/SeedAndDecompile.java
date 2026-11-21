// Apply REA il2cpp seeds (address<TAB>function<TAB>name) and decompile selected functions.
// Script args: <seeds.tsv> <targets.tsv> <out dir> [skipSeeds]
// targets.tsv lines: address<TAB>name. Output: one <name>@<address>.c per target.
import ghidra.app.decompiler.DecompInterface;
import ghidra.app.decompiler.DecompileOptions;
import ghidra.app.decompiler.DecompileResults;
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Function;
import ghidra.program.model.symbol.SourceType;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.FileWriter;
import java.util.ArrayList;
import java.util.List;

public class SeedAndDecompile extends GhidraScript {
	private List<String[]> readTsv(String path) throws Exception {
		List<String[]> rows = new ArrayList<>();
		try (BufferedReader r = new BufferedReader(new FileReader(path))) {
			String line;
			while ((line = r.readLine()) != null) {
				if (line.isEmpty() || line.startsWith("#")) {
					continue;
				}
				rows.add(line.split("\t"));
			}
		}
		return rows;
	}

	@Override
	protected void run() throws Exception {
		String[] args = getScriptArgs();
		boolean skipSeeds = args.length > 3 && args[3].equals("skipSeeds");
		if (!skipSeeds) {
			List<String[]> seeds = readTsv(args[0]);
			int created = 0, failed = 0;
			long start = System.currentTimeMillis();
			// Highest address first so earlier functions do not swallow later entry points.
			seeds.sort((a, b) -> Long.compareUnsigned(Long.decode(b[0]), Long.decode(a[0])));
			for (String[] s : seeds) {
				Address addr = toAddr(s[0]);
				String name = s.length > 2 ? s[2] : null;
				Function f = getFunctionAt(addr);
				if (f == null) {
					if (getInstructionAt(addr) == null) {
						disassemble(addr);
					}
					f = createFunction(addr, name);
				}
				if (f == null) {
					failed++;
					continue;
				}
				if (name != null && f.getName().startsWith("FUN_")) {
					f.setName(name, SourceType.IMPORTED);
				}
				created++;
				if (created % 10000 == 0) {
					println("seeded " + created + " in " + (System.currentTimeMillis() - start) / 1000 + "s");
				}
			}
			println("seeds applied=" + created + " failed=" + failed);
		}

		DecompInterface decomp = new DecompInterface();
		DecompileOptions opts = new DecompileOptions();
		decomp.setOptions(opts);
		decomp.openProgram(currentProgram);
		File outDir = new File(args[2]);
		outDir.mkdirs();
		for (String[] t : readTsv(args[1])) {
			Address addr = toAddr(t[0]);
			Function f = getFunctionAt(addr);
			if (f == null) {
				disassemble(addr);
				f = createFunction(addr, t[1]);
			}
			String fileName = t[1].replaceAll("[^A-Za-z0-9_.$-]", "_") + "@" + t[0] + ".c";
			try (FileWriter w = new FileWriter(new File(outDir, fileName))) {
				if (f == null) {
					w.write("// no function at " + t[0] + "\n");
					continue;
				}
				DecompileResults res = decomp.decompileFunction(f, 180, monitor);
				if (res.decompileCompleted()) {
					w.write(res.getDecompiledFunction().getC());
				}
				else {
					w.write("// decompile failed: " + res.getErrorMessage() + "\n");
				}
			}
		}
		decomp.dispose();
	}
}
