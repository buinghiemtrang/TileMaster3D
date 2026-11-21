// Resolve ARM linker veneers (`movw/movt r12; add r12,r12,pc; bx r12`) and direct thunks (`b target`).
// Headless: -postScript ResolveVeneers.java <addresses.txt> <out.tsv>
// addresses.txt: one Ghidra address (hex) per line. out.tsv: address<TAB>target RVA (hex, without the 0x10000 image base).
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Instruction;
import java.io.*;

public class ResolveVeneers extends GhidraScript {
	@Override
	protected void run() throws Exception {
		String[] args = getScriptArgs();
		long base = currentProgram.getImageBase().getOffset();
		try (BufferedReader r = new BufferedReader(new FileReader(args[0]));
				PrintWriter w = new PrintWriter(new FileWriter(args[1]))) {
			String line;
			while ((line = r.readLine()) != null) {
				line = line.trim();
				if (line.isEmpty()) continue;
				long addr = Long.parseLong(line, 16);
				Address a = toAddr(addr);
				disassemble(a);
				Instruction i0 = getInstructionAt(a);
				if (i0 == null) { w.println(line + "\t?"); continue; }
				long target = -1;
				try {   // a `movw` that is not a veneer (no scalar / no movt) used to abort the whole run
					if (i0.getMnemonicString().startsWith("movw")) {
						Instruction i1 = i0.getNext(), i2 = i1 == null ? null : i1.getNext();
						long lo = i0.getScalar(1).getUnsignedValue();
						long hi = i1.getScalar(1).getUnsignedValue();
						target = ((hi << 16) | lo) + i2.getAddress().getOffset() + 8;
					} else if (i0.getMnemonicString().equals("b")) {
						target = i0.getAddress(0).getOffset();
					}
				} catch (Exception e) { target = -1; }
				w.println(line + "\t" + (target < 0 ? "?" : Long.toHexString(target - base)));
			}
		}
	}
}
