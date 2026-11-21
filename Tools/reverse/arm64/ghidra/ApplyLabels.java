// Apply names from a TSV (address<TAB>name). A function at the address is renamed;
// otherwise a primary label is created (metadata slots, globals).
// Script args: <labels.tsv> [more.tsv ...]
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Function;
import ghidra.program.model.symbol.SourceType;
import ghidra.program.model.symbol.Symbol;

import java.io.BufferedReader;
import java.io.FileReader;

public class ApplyLabels extends GhidraScript {
	@Override
	public void run() throws Exception {
		int funcs = 0, labels = 0, failed = 0;
		for (String path : getScriptArgs()) {
			try (BufferedReader r = new BufferedReader(new FileReader(path))) {
				String line;
				while ((line = r.readLine()) != null) {
					if (line.isEmpty() || line.startsWith("#")) continue;
					String[] p = line.split("\t");
					if (p.length < 2) continue;
					Address a = toAddr(Long.decode(p[0]));
					String name = p[1].replaceAll("[^A-Za-z0-9_$.<>,\\[\\]]", "_");
					try {
						Function f = getFunctionAt(a);
						if (f != null) {
							f.setName(name, SourceType.USER_DEFINED);
							funcs++;
						} else {
							Symbol s = createLabel(a, name, true, SourceType.USER_DEFINED);
							if (s != null) labels++;
						}
					} catch (Exception e) {
						failed++;
					}
				}
			}
		}
		println("functions renamed=" + funcs + " labels=" + labels + " failed=" + failed);
	}
}
