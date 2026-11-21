// Create functions at IL2CPP runtime helper addresses (veneers, thunks and what they jump to), so the
// decompiler calls them instead of inlining their body (an inlined GC write barrier ends in `return`
// and made it drop the rest of the caller as unreachable code).
// Headless: -postScript DefineHelpers.java <addresses.txt> [helper_names.tsv]   (addresses.txt: one Ghidra address in hex per line)
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Instruction;
import java.io.*;
import java.util.*;

public class DefineHelpers extends GhidraScript {
	@Override
	protected void run() throws Exception {
		Set<Long> todo = new LinkedHashSet<>();
		try (BufferedReader r = new BufferedReader(new FileReader(getScriptArgs()[0]))) {
			String line;
			while ((line = r.readLine()) != null) if (!line.trim().isEmpty()) todo.add(Long.parseLong(line.trim(), 16));
		}
		int created = 0;
		List<Long> queue = new ArrayList<>(todo);
		for (int q = 0; q < queue.size(); q++) {
			Address a = toAddr(queue.get(q));
			if (getFunctionAt(a) == null) {
				disassemble(a);
				try { if (createFunction(a, null) != null) created++; } catch (Exception e) {}
			}
			// follow `b target` thunks one level down
			Instruction in = getInstructionAt(a);
			if (in != null && in.getMnemonicString().equals("b") && in.getAddress(0) != null) {
				long t = in.getAddress(0).getOffset();
				if (!todo.contains(t)) { todo.add(t); queue.add(t); }
			}
		}
		// Stable names for the IL2CPP runtime helpers, from an optional second argument: a TSV of
		// <ghidra address hex>\t<name>. These addresses are specific to ONE libil2cpp.so build, see docs/workflow-apk-to-unity.md.
		// Names the cleaner (clean_pseudoc.py) understands: il2cpp_init_metadata, il2cpp_class_init, il2cpp_wbarrier,
		// il2cpp_new, il2cpp_new_array, il2cpp_throw_null, il2cpp_throw_index, il2cpp_throw_overflow (throw_* are marked noreturn).
		if (getScriptArgs().length > 1) {
			try (BufferedReader r = new BufferedReader(new FileReader(getScriptArgs()[1]))) {
				String line;
				while ((line = r.readLine()) != null) {
					String[] n = line.trim().split("\t");
					if (n.length < 2) continue;
					ghidra.program.model.listing.Function f = getFunctionAt(toAddr(Long.parseLong(n[0], 16)));
					if (f == null) { println("no function at " + n[0]); continue; }
					f.setName(n[1], ghidra.program.model.symbol.SourceType.USER_DEFINED);
					if (n[1].startsWith("il2cpp_throw")) f.setNoReturn(true);
				}
			}
		}
		println("helpers defined: " + created + " of " + queue.size());
	}
}
