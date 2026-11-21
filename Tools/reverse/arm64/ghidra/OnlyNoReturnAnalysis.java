// Pre-script: disable every auto-analyzer except the non-returning function analyzers.
import ghidra.app.script.GhidraScript;
import ghidra.framework.options.Options;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.Program;

import java.util.Set;

public class OnlyNoReturnAnalysis extends GhidraScript {
	private static final Set<String> KEEP = Set.of(
		"Non-Returning Functions - Known",
		"Non-Returning Functions - Discovered");

	@Override
	protected void run() throws Exception {
		Options analyzers = currentProgram.getOptions(Program.ANALYSIS_PROPERTIES);
		int off = 0;
		for (String name : analyzers.getOptionNames()) {
			if (name.contains(".") || !(analyzers.getObject(name, null) instanceof Boolean)) {
				continue; // sub-options of an analyzer
			}
			boolean keep = KEEP.contains(name);
			analyzers.setBoolean(name, keep);
			if (!keep) {
				off++;
			}
		}
		println("analyzers disabled=" + off + " kept=" + KEEP);

		// Managed throw helpers never return; marking them seeds the discovery analyzer.
		int marked = 0;
		for (Function f : currentProgram.getFunctionManager().getFunctions(true)) {
			String n = f.getName();
			if (n.contains("ThrowHelper$$Throw") || n.matches(".*\\$\\$Throw[A-Z]\\w*Exception")) {
				f.setNoReturn(true);
				marked++;
			}
		}
		println("throw helpers marked noreturn=" + marked);
	}
}
