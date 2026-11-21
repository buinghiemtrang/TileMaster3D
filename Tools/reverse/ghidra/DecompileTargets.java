// Decompile the functions listed in targets.tsv to <out_dir>/<Class>.c (one file per class).
// Headless: -postScript DecompileTargets.java <targets.tsv> <out_dir>
import ghidra.app.script.GhidraScript;
import ghidra.app.decompiler.DecompInterface;
import ghidra.app.decompiler.DecompileResults;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Function;
import java.io.*;
import java.util.*;

public class DecompileTargets extends GhidraScript {
	@Override
	protected void run() throws Exception {
		String[] args = getScriptArgs();
		File out = new File(args[1]);
		out.mkdirs();
		Address base = currentProgram.getImageBase();
		DecompInterface di = new DecompInterface();
		di.openProgram(currentProgram);
		Map<String, StringBuilder> files = new LinkedHashMap<>();
		int n = 0;
		try (BufferedReader r = new BufferedReader(new FileReader(args[0]))) {
			String line;
			while ((line = r.readLine()) != null) {
				String[] p = line.split("\t");
				long rva = Long.parseLong(p[0]);
				String name = p[1];
				Function f = getFunctionAt(base.add(rva));
				String cls = name.substring(0, name.indexOf("$$"));
				StringBuilder sb = files.computeIfAbsent(cls, k -> new StringBuilder());
				sb.append("// ").append(name).append("  RVA: 0x").append(Long.toHexString(rva)).append('\n');
				if (f == null) { sb.append("// no function\n\n"); continue; }
				DecompileResults res = di.decompileFunction(f, 120, monitor);
				if (res.decompileCompleted()) sb.append(res.getDecompiledFunction().getC()).append('\n');
				else sb.append("// decompile failed: ").append(res.getErrorMessage()).append("\n\n");
				if (++n % 20 == 0) println("decompiled " + n);
			}
		}
		for (Map.Entry<String, StringBuilder> e : files.entrySet()) {
			try (PrintWriter w = new PrintWriter(new File(out, e.getKey() + ".c"), "UTF-8")) {
				w.print(e.getValue());
			}
		}
		println("decompiled " + n + " functions into " + files.size() + " files");
	}
}
