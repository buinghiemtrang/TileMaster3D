// List unconditional tail calls (`b <other function>`) inside given functions.
// The decompiler sometimes drops a whole call block when the call is a tail branch (QuitPopup.Awake,
// TryAgainPopup.Awake lost their second AddListener), so every hit must be checked against the pseudo-C.
// Headless: -postScript TailCalls.java <in.tsv: ghidra_start_hex \t ghidra_end_hex \t label> <out.tsv>
// Output: label \t start \t branch_address \t target (ghidra addresses, hex). Not game specific.
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Instruction;
import ghidra.program.model.symbol.FlowType;
import java.io.*;
import java.nio.charset.StandardCharsets;

public class TailCalls extends GhidraScript {
	@Override
	protected void run() throws Exception {
		String[] a = getScriptArgs();
		try (BufferedReader r = new BufferedReader(new InputStreamReader(new FileInputStream(a[0]), StandardCharsets.UTF_8));
			PrintWriter w = new PrintWriter(new OutputStreamWriter(new FileOutputStream(a[1]), StandardCharsets.UTF_8))) {
			String line;
			int n = 0;
			while ((line = r.readLine()) != null) {
				String[] f = line.split("\t");
				long start = Long.parseLong(f[0], 16), end = Long.parseLong(f[1], 16);
				Address p = toAddr(start);
				while (p.getOffset() < end) {
					Instruction in = getInstructionAt(p);
					if (in == null) { disassemble(p); in = getInstructionAt(p); }
					if (in == null) break;
					FlowType ft = in.getFlowType();
					if (ft.isJump() && !ft.isConditional() && !ft.isCall()) {
						for (Address t : in.getFlows()) {
							long to = t.getOffset();
							if (to < start || to >= end) {
								w.println(f[2] + "\t" + Long.toHexString(start) + "\t" + Long.toHexString(p.getOffset()) + "\t" + Long.toHexString(to));
							}
						}
					}
					p = p.add(in.getLength());
				}
				if (++n % 2000 == 0) println("functions " + n);
			}
		}
	}
}
