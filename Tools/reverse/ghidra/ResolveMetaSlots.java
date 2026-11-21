// Resolve IL2CPP metadata-usage slots referenced in pseudo-C as `&UNK_<a> + _UNK_<b>`.
// slot (Ghidra address) = a + int32 at b; the word stored at slot is the metadata cell address (printed minus imageBase).
// Headless: -postScript ResolveMetaSlots.java <pairs.txt> <out.tsv>   (pairs.txt: "<a hex> <b hex>" per line)
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import java.io.*;
import java.nio.file.*;

public class ResolveMetaSlots extends GhidraScript {
	@Override
	protected void run() throws Exception {
		String[] a = getScriptArgs();
		long base = currentProgram.getImageBase().getOffset();
		try (PrintWriter w = new PrintWriter(a[1])) {
			for (String line : Files.readAllLines(Paths.get(a[0]))) {
				String[] p = line.trim().split("\\s+");
				if (p.length < 2) continue;
				long u = Long.parseLong(p[0], 16), l = Long.parseLong(p[1], 16);
				int lit = currentProgram.getMemory().getInt(toAddr(l));
				long slot = (u + lit) & 0xffffffffL;
				int cell;
				try { cell = currentProgram.getMemory().getInt(toAddr(slot)); } catch (Exception e) { continue; } // .bss flag bytes
				w.println(p[0] + "_" + p[1] + "\t" + (slot - base) + "\t" + (((long) cell & 0xffffffffL) - base));
			}
		}
	}
}
