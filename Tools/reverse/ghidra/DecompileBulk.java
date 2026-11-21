// Bulk decompile: like DecompileTargets.java but made for tens of thousands of functions.
//  - a class with more than 500 methods is split into <Class>.c, <Class>.p2.c, ... (huge generic classes such as
//    System.Array have 15k instantiations). A progress line is printed every minute.
//  - targets.tsv must be grouped by class (make_bulk_targets.py does that); each class file is written as soon as its
//    last function is done, so a crash or a stop loses at most one class and a re-run skips finished classes.
//  - `< > | : ? * "` in file names become the full-width twins U+FF1C FF1E FF5C FF1A FF1F FF0A FF02 (Windows forbids them;
//    restore_names.py maps them back).
// Headless: -postScript DecompileBulk.java <targets.tsv> <out_dir> [timeout_seconds_per_function] [create|-] [suffix]
//  `create`: when no function exists at an address, disassemble and create one first (for runtime helpers that
//  Il2CppDumper does not name; use with -readOnly so the project is not changed).
import ghidra.app.script.GhidraScript;
import ghidra.app.decompiler.DecompInterface;
import ghidra.app.decompiler.DecompileResults;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Function;
import java.io.*;
import java.util.*;

public class DecompileBulk extends GhidraScript {
	static String safe(String s) {
		return s.replace('<', '＜').replace('>', '＞').replace('|', '｜').replace(':', '：')
			.replace('?', '？').replace('*', '＊').replace('"', '＂');
	}

	@Override
	protected void run() throws Exception {
		String[] args = getScriptArgs();
		File out = new File(args[1]);
		out.mkdirs();
		int timeout = args.length > 2 ? Integer.parseInt(args[2]) : 60;
		boolean create = args.length > 3 && args[3].equals("create");
		String suffix = args.length > 4 ? args[4] : "";   // e.g. ".gen": <Class>.gen.c, <Class>.gen.p2.c (second batch)
		Address base = currentProgram.getImageBase();
		DecompInterface di = new DecompInterface();
		di.openProgram(currentProgram);
		List<String[]> rows = new ArrayList<>();
		try (BufferedReader r = new BufferedReader(new FileReader(args[0]))) {
			String line;
			while ((line = r.readLine()) != null) rows.add(line.split("\t"));
		}
		long t0 = System.currentTimeMillis();
		int n = 0, classes = 0, skipped = 0, failed = 0, i = 0;
		final int CHUNK = 500;   // a class with more methods is written as <Class>.c, <Class>.p2.c, <Class>.p3.c ...
		long lastLog = t0;
		while (i < rows.size()) {
			String name0 = rows.get(i)[1];
			String cls = name0.substring(0, name0.indexOf("$$"));
			int j = i;
			while (j < rows.size() && rows.get(j)[1].startsWith(cls + "$$")) j++;
			for (int c0 = i, part = 1; c0 < j; c0 += CHUNK, part++) {
				int c1 = Math.min(j, c0 + CHUNK);
				String base0 = safe(cls);
				if (base0.length() > 140) {   // Windows MAX_PATH: cut long generic names, keep a hash, record the full name
					String cut = base0.substring(0, 120) + "~" + String.format("%08x", cls.hashCode());
					try (PrintWriter lw = new PrintWriter(new FileWriter(new File(out, "_long_names.tsv"), true))) { lw.println(cut + "\t" + cls); }
					base0 = cut;
				}
				String fname = base0 + suffix + (part == 1 ? "" : ".p" + part);
				File dest = new File(out, fname + ".c");
				if (dest.exists()) { skipped++; continue; }
				StringBuilder sb = new StringBuilder();
				int streak = 0;   // consecutive timeouts in this chunk; after 8 the rest is skipped (retry_failed.py redoes it)
				for (int k = c0; k < c1; k++) {
					long rva = Long.parseLong(rows.get(k)[0]);
					if (streak >= 8) {
						sb.append("// ").append(rows.get(k)[1]).append("  RVA: 0x").append(Long.toHexString(rva)).append('\n');
						sb.append("// decompile failed: skipped after 8 timeouts in a row\n\n");
						failed++;
						continue;
					}
					Function f = getFunctionAt(base.add(rva));
					if (f == null && create) {
						try { disassemble(base.add(rva)); f = createFunction(base.add(rva), null); } catch (Exception e) { f = null; }
					}
					sb.append("// ").append(rows.get(k)[1]).append("  RVA: 0x").append(Long.toHexString(rva)).append('\n');
					if (f == null) { sb.append("// no function\n\n"); continue; }
					DecompileResults res = di.decompileFunction(f, timeout, monitor);
					if (res.decompileCompleted()) { streak = 0; sb.append(res.getDecompiledFunction().getC()).append('\n'); }
					else { failed++; streak++; sb.append("// decompile failed: ").append(res.getErrorMessage()).append("\n\n"); }
					n++;
					if (System.currentTimeMillis() - lastLog > 60000) {
						lastLog = System.currentTimeMillis();
						println("progress " + n + " functions, " + classes + " files, " + failed + " failed, row " + k + "/" + rows.size()
							+ ", " + (lastLog - t0) / 1000 + "s");
					}
				}
				File tmp = new File(out, fname + ".c.part");
				try (PrintWriter w = new PrintWriter(tmp, "UTF-8")) { w.print(sb); }
				tmp.renameTo(dest);
				classes++;
			}
			i = j;
		}
		println("decompiled " + n + " functions into " + classes + " files (skipped " + skipped + " existing, " + failed
			+ " failed) in " + (System.currentTimeMillis() - t0) / 1000 + "s");
	}
}
