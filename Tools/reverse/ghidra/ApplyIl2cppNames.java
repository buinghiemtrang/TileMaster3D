// Create a function and a label for every IL2CPP method address (names.tsv: address<TAB>name).
// Headless: -postScript ApplyIl2cppNames.java <names.tsv>
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.symbol.SourceType;
import java.io.BufferedReader;
import java.io.FileReader;

public class ApplyIl2cppNames extends GhidraScript {
	@Override
	protected void run() throws Exception {
		String path = getScriptArgs()[0];
		Address base = currentProgram.getImageBase();
		int n = 0, created = 0, failed = 0;
		try (BufferedReader r = new BufferedReader(new FileReader(path))) {
			String line;
			while ((line = r.readLine()) != null) {
				int t = line.indexOf('\t');
				if (t < 0) continue;
				Address a = base.add(Long.parseLong(line.substring(0, t)));
				String name = line.substring(t + 1);
				try {
					if (getFunctionAt(a) == null && createFunction(a, name) != null) created++;
					else if (getFunctionAt(a) != null) getFunctionAt(a).setName(name, SourceType.USER_DEFINED);
				} catch (Exception e) {
					failed++;
					try { createLabel(a, name, true, SourceType.USER_DEFINED); } catch (Exception e2) {}
				}
				if (++n % 50000 == 0) println("names: " + n);
			}
		}
		println("done: " + n + " names, " + created + " functions created, " + failed + " failed");
	}
}
