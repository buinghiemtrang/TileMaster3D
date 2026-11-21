// Print the disassembly of a function (or N instructions from an address).
// Headless: -postScript ListInstr.java <ghidra address hex> [max instructions]
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Instruction;

public class ListInstr extends GhidraScript {
	@Override
	protected void run() throws Exception {
		String[] a = getScriptArgs();
		Address p = toAddr(Long.parseLong(a[0], 16));
		int max = a.length > 1 ? Integer.parseInt(a[1]) : 200;
		disassemble(p);
		for (int i = 0; i < max; i++) {
			Instruction in = getInstructionAt(p);
			if (in == null) { disassemble(p); in = getInstructionAt(p); }
			if (in == null) { println("?? " + p); break; }
			println(String.format("%08x  %s", p.getOffset(), in));
			p = p.add(in.getLength());
		}
	}
}
