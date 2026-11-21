// Split code absorbed through tail branches out of target functions.
// Script args: <targets.tsv>. For each target, every body range that does not
// contain the entry point becomes its own function, then the body is recomputed;
// repeats until the body is a single range.
import ghidra.app.cmd.function.CreateFunctionCmd;
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.address.AddressRange;
import ghidra.program.model.listing.Function;

import java.io.BufferedReader;
import java.io.FileReader;
import java.util.ArrayList;
import java.util.List;

public class FixBodies extends GhidraScript {
	@Override
	protected void run() throws Exception {
		List<Address> targets = new ArrayList<>();
		try (BufferedReader r = new BufferedReader(new FileReader(getScriptArgs()[0]))) {
			String line;
			while ((line = r.readLine()) != null) {
				if (!line.isEmpty()) {
					targets.add(toAddr(line.split("\t")[0]));
				}
			}
		}
		int split = 0, fixed = 0;
		for (Address entry : targets) {
			Function f = getFunctionAt(entry);
			if (f == null) {
				continue;
			}
			for (int pass = 0; pass < 20; pass++) {
				List<Address> starts = new ArrayList<>();
				for (AddressRange range : f.getBody()) {
					if (!range.contains(entry)) {
						starts.add(range.getMinAddress());
					}
				}
				if (starts.isEmpty()) {
					break;
				}
				for (Address s : starts) {
					if (getFunctionAt(s) == null && createFunction(s, null) != null) {
						split++;
					}
				}
				CreateFunctionCmd.fixupFunctionBody(currentProgram, f, monitor);
				fixed++;
			}
		}
		println("absorbed regions split=" + split + " body fixups=" + fixed);
	}
}
