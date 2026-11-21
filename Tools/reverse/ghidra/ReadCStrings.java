// Read what PC-relative address expressions point at, as Ghidra prints them: `_UNK_<lit> + 0x<pc>` (runtime code) or
// `&UNK_<pc> + _UNK_<lit>` (managed code). address = int32 at <lit> + <pc> (both Ghidra addresses).
// Output per pair: "<lit>_<pc>\tS\t<string>" when the address holds a printable C string (>= 2 chars),
//                  else "<lit>_<pc>\tA\t<address hex>" so callers can print it as DAT_<address>.
// Headless: -postScript ReadCStrings.java <pairs.txt> <out.tsv>   (pairs.txt: "<lit hex> <pc hex>" per line)
import ghidra.app.script.GhidraScript;
import java.io.*;
import java.nio.file.*;

public class ReadCStrings extends GhidraScript {
	@Override
	protected void run() throws Exception {
		String[] a = getScriptArgs();
		try (PrintWriter w = new PrintWriter(a[1], "UTF-8")) {
			for (String line : Files.readAllLines(Paths.get(a[0]))) {
				String[] p = line.trim().split("\\s+");
				if (p.length < 2) continue;
				long addr;
				try {
					long lit = Long.parseLong(p[0], 16), pc = Long.parseLong(p[1], 16);
					addr = ((currentProgram.getMemory().getInt(toAddr(lit)) & 0xffffffffL) + pc) & 0xffffffffL;
				} catch (Exception e) { continue; }
				StringBuilder sb = new StringBuilder();
				try {
					for (int i = 0; i < 400; i++) {
						int b = currentProgram.getMemory().getByte(toAddr(addr + i)) & 0xff;
						if (b == 0) break;
						if (b < 0x20 || b > 0x7e) { sb = null; break; }
						sb.append((char) b);
					}
				} catch (Exception e) { sb = null; }
				if (sb != null && sb.length() >= 2) w.println(p[0] + "_" + p[1] + "\tS\t" + sb);
				else w.println(p[0] + "_" + p[1] + "\tA\t" + Long.toHexString(addr));
			}
		}
	}
}
