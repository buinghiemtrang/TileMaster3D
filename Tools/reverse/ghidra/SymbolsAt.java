// Print the symbols, the memory block and the 8 bytes found at the given RVAs (image base is added).
// Headless: -postScript SymbolsAt.java <rva hex> [<rva hex> ...]
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.symbol.Symbol;

public class SymbolsAt extends GhidraScript {
	@Override
	protected void run() throws Exception {
		Address base = currentProgram.getImageBase();
		for (String a : getScriptArgs()) {
			Address ad = base.add(Long.parseLong(a, 16));
			StringBuilder sb = new StringBuilder("SYM " + a + " -> " + ad + " block=" + (currentProgram.getMemory().getBlock(ad) == null ? "none" : currentProgram.getMemory().getBlock(ad).getName()));
			for (Symbol s : currentProgram.getSymbolTable().getSymbols(ad)) sb.append(" [").append(s.getName()).append("]");
			try { sb.append(" word=0x").append(Long.toHexString(currentProgram.getMemory().getInt(ad) & 0xffffffffL)); } catch (Exception e) { sb.append(" word=?"); }
			println(sb.toString());
		}
	}
}
