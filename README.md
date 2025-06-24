
```markdown
# Algorithmic Plagiarism Detector

A C#-based tool that automates the detection of algorithmic plagiarism among student submissions using graph theory, similarity metrics, and Excel file processing.

## 📌 Features

- Reads Excel files containing pairwise file similarity data.
- Builds an undirected graph to represent file relationships.
- Detects connected components representing groups of similar submissions.
- Calculates and exports statistics for each group (e.g., average similarity).
- Constructs a Minimum Spanning Tree (MST) to highlight most representative plagiarism chains.
- Generates two Excel reports: one for component stats, and one for MST structures.

## 🧠 How It Works

1. **Data Input**: Reads from Excel sheets with similarity scores and common lines between file pairs.
2. **Graph Construction**: Constructs an undirected graph where nodes are file identifiers, and edges represent similarity.
3. **Grouping**: Uses DFS to find connected components—interpreted as potentially plagiarized groups.
4. **Statistical Analysis**: Calculates average similarity within each group.
5. **MST Creation**: For each group, a Minimum Spanning Tree (Kruskal-like) is generated to identify critical plagiarism paths.
6. **Excel Output**:
   - `*_Stat.xlsx`: Component IDs, their members, average similarity, and count.
   - `*_MST.xlsx`: File pairs and matching line counts.

## 📂 Project Structure

```

📁 PlagiarismValidation
├── Program.cs                  # Main entry point
├── FileSimilarityAnalyzer.cs  # Core logic: grouping, MST building, etc.
├── ExcelHelper.cs             # Reading/writing Excel files
├── Entry.cs                   # Represents file similarity entry
├── Edge.cs                    # Graph edge with weights
├── Component.cs               # Grouping structure
├── Sort.cs                    # Custom sort utilities

```

## 🚀 Getting Started

### Prerequisites

- .NET Core SDK
- NuGet packages:
  - `EPPlus` (for Excel handling)
  - `DocumentFormat.OpenXml`
  - `ClosedXML`

### Build & Run

1. Open the solution: `PlagiarismValidation.sln`
2. Add your test Excel files to the project or update paths in `Program.cs`
3. Run the project

### Sample Test Case Input Format (Excel)

| File1           | File2           | Matched Lines |
|----------------|----------------|----------------|
| fileA (80%)     | fileB (82%)     | 45             |
| fileC (40%)     | fileD (42%)     | 5              |

> The system infers file similarity based on `%` values and number of matched lines.

## 📈 Output

- `Results/{CaseName}_Stat.xlsx`: Groups with file IDs and average similarities
- `Results/{CaseName}_MST.xlsx`: Plagiarism chains (minimal connecting lines)

## 🧪 Example Use Case

You are a university instructor with multiple student submissions for programming assignments. This tool will help you:

- Quickly identify clusters of suspiciously similar files.
- Visualize which submissions are most central in a group (via MST).
- Generate detailed Excel reports for documentation.

## 🔧 Customization

- Adjust similarity thresholds and logic in `FileSimilarityAnalyzer.cs`
- Modify Excel formatting in `ExcelHelper.cs`

## 👨‍💻 Author

Developed by [Ahmed Mahmoud](https://github.com/ahmedev192) as part of a graduation or research project focused on academic integrity and automation.

---

```
