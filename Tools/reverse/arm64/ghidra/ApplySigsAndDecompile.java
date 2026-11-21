// Create il2cpp class structs, apply method signatures, then decompile targets.
// Script args: <structs.tsv|-> <sigs.tsv|-> <targets.tsv> <out dir>
// structs.tsv: struct<TAB>size<TAB>offset<TAB>field<TAB>token
// sigs.tsv: address<TAB>name<TAB>ret token<TAB>name:token;...
import ghidra.app.decompiler.DecompInterface;
import ghidra.app.decompiler.DecompileOptions;
import ghidra.app.decompiler.DecompileResults;
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.data.*;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.Function.FunctionUpdateType;
import ghidra.program.model.listing.ParameterImpl;
import ghidra.program.model.listing.ReturnParameterImpl;
import ghidra.program.model.listing.Variable;
import ghidra.program.model.symbol.SourceType;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.FileWriter;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class ApplySigsAndDecompile extends GhidraScript {
	private static final CategoryPath CAT = new CategoryPath("/il2cpp");
	private final Map<String, DataType> structs = new HashMap<>();
	private DataTypeManager dtm;

	private List<String[]> readTsv(String path) throws Exception {
		List<String[]> rows = new ArrayList<>();
		if (path.equals("-")) {
			return rows;
		}
		try (BufferedReader r = new BufferedReader(new FileReader(path))) {
			String line;
			while ((line = r.readLine()) != null) {
				if (!line.isEmpty() && !line.startsWith("#")) {
					rows.add(line.split("\t", -1));
				}
			}
		}
		return rows;
	}

	private DataType floats(int n) {
		String name = "float" + n + "_il2cpp";
		DataType existing = dtm.getDataType(CAT, name);
		if (existing != null) {
			return existing;
		}
		StructureDataType s = new StructureDataType(CAT, name, 0, dtm);
		String[] names = { "x", "y", "z", "w" };
		for (int i = 0; i < n; i++) {
			s.add(FloatDataType.dataType, 4, names[i], null);
		}
		return dtm.addDataType(s, DataTypeConflictHandler.REPLACE_HANDLER);
	}

	private DataType type(String tok) {
		switch (tok) {
			case "v": return VoidDataType.dataType;
			case "b": return BooleanDataType.dataType;
			case "i1": return SignedByteDataType.dataType;
			case "u1": return ByteDataType.dataType;
			case "i2": return ShortDataType.dataType;
			case "u2": return UnsignedShortDataType.dataType;
			case "i4": return IntegerDataType.dataType;
			case "u4": return UnsignedIntegerDataType.dataType;
			case "i8": return LongLongDataType.dataType;
			case "u8": return UnsignedLongLongDataType.dataType;
			case "f": return FloatDataType.dataType;
			case "d": return DoubleDataType.dataType;
			case "F2": return floats(2);
			case "F3": return floats(3);
			case "F4": return floats(4);
			default:
				if (tok.startsWith("P:")) {
					DataType s = structs.get(tok.substring(2));
					if (s != null) {
						return new PointerDataType(s, dtm);
					}
				}
				return new PointerDataType(VoidDataType.dataType, dtm);
		}
	}

	@Override
	protected void run() throws Exception {
		String[] args = getScriptArgs();
		dtm = currentProgram.getDataTypeManager();

		Map<String, List<String[]>> fieldsByStruct = new HashMap<>();
		for (String[] r : readTsv(args[0])) {
			fieldsByStruct.computeIfAbsent(r[0], k -> new ArrayList<>()).add(r);
		}
		for (Map.Entry<String, List<String[]>> e : fieldsByStruct.entrySet()) {
			int size = Integer.parseInt(e.getValue().get(0)[1]);
			StructureDataType s = new StructureDataType(CAT, e.getKey(), size, dtm);
			for (String[] f : e.getValue()) {
				int off = Integer.parseInt(f[2]);
				DataType dt = type(f[4]);
				int len = Math.min(dt.getLength(), size - off);
				if (len <= 0) {
					continue;
				}
				try {
					s.replaceAtOffset(off, dt, len, f[3], null);
				}
				catch (Exception ex) {
					// overlapping or oversize field: keep the struct usable
				}
			}
			structs.put(e.getKey(), dtm.addDataType(s, DataTypeConflictHandler.REPLACE_HANDLER));
		}
		// Existing structs from an earlier run of this script.
		if (fieldsByStruct.isEmpty()) {
			dtm.getCategory(CAT);
		}
		println("structs=" + structs.size());

		int applied = 0, failed = 0;
		for (String[] sig : readTsv(args[1])) {
			Function f = getFunctionAt(toAddr(sig[0]));
			if (f == null) {
				failed++;
				continue;
			}
			try {
				List<Variable> params = new ArrayList<>();
				if (!sig[3].isEmpty()) {
					for (String p : sig[3].split(";")) {
						int c = p.indexOf(':');
						params.add(new ParameterImpl(p.substring(0, c), type(p.substring(c + 1)),
							currentProgram));
					}
				}
				f.updateFunction(null, new ReturnParameterImpl(type(sig[2]), currentProgram),
					params, FunctionUpdateType.DYNAMIC_STORAGE_ALL_PARAMS, true,
					SourceType.USER_DEFINED);
				applied++;
			}
			catch (Exception ex) {
				failed++;
			}
		}
		println("signatures applied=" + applied + " failed=" + failed);

		DecompInterface decomp = new DecompInterface();
		decomp.setOptions(new DecompileOptions());
		decomp.openProgram(currentProgram);
		File outDir = new File(args[3]);
		outDir.mkdirs();
		for (String[] t : readTsv(args[2])) {
			Address addr = toAddr(t[0]);
			Function f = getFunctionAt(addr);
			String fileName = t[1].replaceAll("[^A-Za-z0-9_.$-]", "_") + "@" + t[0] + ".c";
			try (FileWriter w = new FileWriter(new File(outDir, fileName))) {
				if (f == null) {
					w.write("// no function at " + t[0] + "\n");
					continue;
				}
				DecompileResults res = decomp.decompileFunction(f, 180, monitor);
				w.write(res.decompileCompleted() ? res.getDecompiledFunction().getC()
						: "// decompile failed: " + res.getErrorMessage() + "\n");
			}
		}
		decomp.dispose();
	}
}
