// Create functions at every call target reached from existing functions, repeating
// until no new functions appear, so the decompiler stops treating calls as branches.
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.FunctionIterator;
import ghidra.program.model.listing.Instruction;
import ghidra.program.model.listing.InstructionIterator;
import ghidra.program.model.symbol.FlowType;

import java.util.ArrayDeque;
import java.util.Deque;
import java.util.HashSet;
import java.util.Set;

public class CreateCallTargets extends GhidraScript {
	@Override
	protected void run() throws Exception {
		Deque<Function> work = new ArrayDeque<>();
		FunctionIterator it = currentProgram.getFunctionManager().getFunctions(true);
		while (it.hasNext()) {
			work.add(it.next());
		}
		Set<Address> tried = new HashSet<>();
		int created = 0;
		while (!work.isEmpty()) {
			monitor.checkCancelled();
			Function f = work.poll();
			InstructionIterator ins = currentProgram.getListing().getInstructions(f.getBody(), true);
			while (ins.hasNext()) {
				Instruction i = ins.next();
				FlowType flow = i.getFlowType();
				if (!flow.isCall()) {
					continue;
				}
				for (Address dest : i.getFlows()) {
					if (getFunctionAt(dest) != null || !tried.add(dest)) {
						continue;
					}
					if (getInstructionAt(dest) == null) {
						disassemble(dest);
					}
					Function nf = createFunction(dest, null);
					if (nf != null) {
						created++;
						work.add(nf);
					}
				}
			}
		}
		println("call-target functions created=" + created);
	}
}
