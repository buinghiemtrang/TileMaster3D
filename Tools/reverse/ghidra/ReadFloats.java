import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
public class ReadFloats extends GhidraScript {
	protected void run() throws Exception {
		Address base = currentProgram.getImageBase();
		println("imageBase " + base);
		for (String s : getScriptArgs()) {
			long rva = Long.decode(s);
			Address a = base.add(rva);
			StringBuilder sb = new StringBuilder("RVA 0x" + Long.toHexString(rva) + ":");
			for (int i = 0; i < 4; i++) {
				int w = currentProgram.getMemory().getInt(a.add(4 * i));
				sb.append(' ').append(Float.intBitsToFloat(w)).append("(0x").append(Integer.toHexString(w)).append(')');
			}
			println(sb.toString());
		}
	}
}
