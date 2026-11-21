// Mark non-returning functions reachable from the targets, then trim bodies.
// Script args: <targets.tsv> [max depth, default 8]
// A path returns when it reaches RET. It does not return when it calls a
// non-returning function, falls into another function's entry, or runs off
// decoded code. A function is non-returning when no path from its entry returns.
import ghidra.app.cmd.function.CreateFunctionCmd;
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.Instruction;
import ghidra.program.model.symbol.FlowType;

import java.io.BufferedReader;
import java.io.FileReader;
import java.util.ArrayDeque;
import java.util.Deque;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Set;

public class MarkNoReturnTargeted extends GhidraScript {
	private static final Set<String> KNOWN = Set.of("__cxa_throw", "__cxa_rethrow", "abort",
		"__stack_chk_fail", "_Unwind_Resume", "exit", "_exit", "__cxa_bad_cast",
		"__cxa_bad_typeid", "__cxa_call_unexpected", "_ZSt9terminatev");
	private final Map<Address, Boolean> returns = new HashMap<>();
	private final Set<Address> inProgress = new HashSet<>();
	private int maxDepth;

	private boolean knownNoReturn(Function f) {
		Function t = f.isThunk() ? f.getThunkedFunction(true) : f;
		String n = t.getName();
		return f.hasNoReturn() || KNOWN.contains(n) || n.contains("ThrowHelper$$Throw") ||
			n.matches(".*\\$\\$Throw[A-Z]\\w*Exception");
	}

	private boolean doesReturn(Function f, int depth) throws Exception {
		Address entry = f.getEntryPoint();
		Boolean cached = returns.get(entry);
		if (cached != null) {
			return cached;
		}
		if (knownNoReturn(f)) {
			returns.put(entry, false);
			return false;
		}
		if (f.isExternal() || f.isThunk() || depth > maxDepth || !inProgress.add(entry)) {
			return true; // unknown or recursive: assume it returns
		}
		boolean found = false;
		Deque<Address> work = new ArrayDeque<>();
		Set<Address> seen = new HashSet<>();
		work.add(entry);
		while (!work.isEmpty() && !found) {
			monitor.checkCancelled();
			Address a = work.poll();
			if (!seen.add(a)) {
				continue;
			}
			if (!a.equals(entry) && getFunctionAt(a) != null) {
				continue; // fell into another function
			}
			Instruction ins = getInstructionAt(a);
			if (ins == null) {
				ins = disassemble(a) ? getInstructionAt(a) : null;
				if (ins == null) {
					continue;
				}
			}
			FlowType flow = ins.getFlowType();
			if (flow.isTerminal()) {
				found = true;
				break;
			}
			if (flow.isCall()) {
				boolean calleeReturns = true;
				for (Address dest : ins.getFlows()) {
					Function callee = getFunctionAt(dest);
					if (callee != null && !doesReturn(callee, depth + 1)) {
						calleeReturns = false;
					}
				}
				if (calleeReturns && ins.getFallThrough() != null) {
					work.add(ins.getFallThrough());
				}
				continue;
			}
			if (flow.isJump()) {
				for (Address dest : ins.getFlows()) {
					Function tail = getFunctionAt(dest);
					if (tail != null && !dest.equals(entry)) {
						if (doesReturn(tail, depth + 1)) {
							found = true; // tail call to a returning function
						}
					}
					else {
						work.add(dest);
					}
				}
				if (flow.isConditional() && ins.getFallThrough() != null) {
					work.add(ins.getFallThrough());
				}
				if (flow.isComputed() && ins.getFlows().length == 0) {
					found = true; // indirect jump: treat as a tail call that returns
				}
				continue;
			}
			if (ins.getFallThrough() != null) {
				work.add(ins.getFallThrough());
			}
		}
		inProgress.remove(entry);
		returns.put(entry, found);
		if (!found) {
			f.setNoReturn(true);
		}
		return found;
	}

	@Override
	protected void run() throws Exception {
		String[] args = getScriptArgs();
		maxDepth = args.length > 1 ? Integer.parseInt(args[1]) : 8;
		Deque<Function> targets = new ArrayDeque<>();
		try (BufferedReader r = new BufferedReader(new FileReader(args[0]))) {
			String line;
			while ((line = r.readLine()) != null) {
				Function f = line.isEmpty() ? null : getFunctionAt(toAddr(line.split("\t")[0]));
				if (f != null) {
					targets.add(f);
				}
			}
		}
		// Decide callees of every target (and the targets themselves).
		for (Function t : targets) {
			doesReturn(t, 0);
			for (Function callee : t.getCalledFunctions(monitor)) {
				doesReturn(callee, 1);
			}
		}
		long noRet = returns.values().stream().filter(v -> !v).count();
		println("functions examined=" + returns.size() + " marked noreturn=" + noRet);
		// Trim bodies that ran past non-returning calls and split out absorbed callees.
		for (Function t : targets) {
			CreateFunctionCmd.fixupFunctionBody(currentProgram, t, monitor);
		}
		int created = 0;
		for (Function t : targets) {
			for (Instruction ins : currentProgram.getListing().getInstructions(t.getBody(), true)) {
				if (!ins.getFlowType().isCall()) {
					continue;
				}
				for (Address dest : ins.getFlows()) {
					if (getFunctionAt(dest) != null) {
						continue;
					}
					Function owner = getFunctionContaining(dest);
					if (owner != null) {
						CreateFunctionCmd.fixupFunctionBody(currentProgram, owner, monitor);
					}
					if (getFunctionAt(dest) == null && createFunction(dest, null) != null) {
						created++;
					}
				}
			}
		}
		println("callee functions created after trimming=" + created);
	}
}
