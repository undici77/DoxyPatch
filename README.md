[![Build and Release](https://github.com/undici77/DoxyPatch/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/undici77/DoxyPatch/actions/workflows/dotnet-desktop.yml)

# **DoxyPatch**

A tool for generating and verifying **Doxygen** comments for C, C++, and C# code.

## **Introduction**

**DoxyPatch** is a command-line tool designed to simplify the creation and verification of **Doxygen** comments for C, C++, and C# code. It utilizes a combination of regular expressions and **AI-powered models** to generate high-quality comments that adhere to the **Doxygen syntax**.

## **Features**

* Generates empty **Doxygen** comment templates for functions and methods without **Ollama**
* Generates filled **Doxygen** comments with **Ollama** enabled, using **AI-powered models** to create accurate and informative comments
* Verifies consistency between function names and **Doxygen** comments
* Checks for empty comments (to compile manually)
* Supports C, C++, and C# languages

## **Long Story Short**

**DoxyPatch** was born out of the need for a tool to simplify the creation and verification of **Doxygen** comments. Initially developed as an external tool due to the lack of support in embedded compilers/IDEs, it has evolved over time to support multiple languages and **AI-powered models**.

## **Requirements**

* **.NET 9** or later 
* **Ollama AI model** (download instructions below)
* 4GB of RAM on the GPU (for running the **AI model**)

## **Compile Project from Source Code**

1. Install **.NET 9 SDK**
2. Clone this repository: `git clone https://github.com/undici77/DoxyPatch.git`
3. Run `build_all.sh` or `build_all.ps1`: Files available in `artifacts` folder 

## **Installation**

1. Install the Ollama AI backend: follow the instructions on the [Ollama website](https://ollama.ai/)
2. Unzip file based on your architecture
3. Folder should be like:

```markdown
.
|-- DoxyPatch
|-- DoxyPatch.ini
`-- Models
    |-- doxypatch
    `-- doxypatch-with-context
```

**Note:** When running **DoxyPatch** with the **Ollama** feature for the first time, if the required models (`DoxyPatch` and `DoxyPatch-with-context`) are not present in the `Models` folder, they will be automatically downloaded. This ensures that you have the necessary models to take full advantage of the **AI-powered** capabilities of **DoxyPatch**.

## **Usage**

```markdown
Usage: DoxyPatch [file or directory] <options>

Arguments:
    [file or directory]
        Specify the file or directory path to process.

Options:
    h, --help
        Show this help message and exit.
    r, --recursive
        Process directories recursively, including all subdirectories and files.
    o, --ollama
        Enable **Ollama** mode to automatically generate **Doxygen** fields for the specified files or directory.
    b, --rebuild
        Rebuild existing **Doxygen** fields, overwriting any previously generated documentation.
    c, --with-context
        Pass the entire source code as context for more accurate documentation generation (experimental feature).
    m, --dry-mode
        Run **DoxyPatch** in dry mode, simulating the documentation generation process without making any actual changes.
    d, --delay
        Specify a delay in seconds between processing files to avoid overheating GPU and CPU.
```

## **Configuration**

The `DoxyPatch.ini` is automatically generated at first start and contains configuration settings for the tool. You can modify this file to customize the behavior of **DoxyPatch**.

```ini
[Ollama]
Address="http://localhost:11434"
ModelName="DoxyPatch:latest"
ModelNameWithContext="DoxyPatch-with-context:latest"
PrePrompt="Please provide your best effort, in **English**, adhering to the rules for this method written in '{LANG}':"
PrePromptWithClass="Please provide your best effort, in **English**, adhering to the rules for this '{CLASS}' class method written in '{LANG}':"
```

Note: `{CLASS}` and `{LANG}` are placeholders that will be replaced with the actual class name and language being processed.

## **Integration with IDEs**

**DoxyPatch** is designed as an external tool, which means it can be easily integrated into any Integrated Development Environment (IDE) that supports external tools. This allows you to use **DoxyPatch's** features directly from within your favorite IDE.

## **Models**

The primary purpose of having an external model is to allow users to experiment with different parameters, models, and prompts to improve the quality of generated comments.

These parameters control various aspects of the model's behavior, such as:

* `FROM`: Specifies the base model
* `temperature`: Controls randomness; lower values make responses more deterministic.  
* `top_p` / `top_k`: Manage token selection for balanced creativity and coherence.  
* `num_ctx`: Defines the max input size.  
* `num_predict`: Limits response length.  
* `SYSTEM`: Prompt to defines the model's behavior.

For more information on customizing these parameters, adding new ones, or learning about other advanced features, please refer to the [Ollama Model File documentation](https://github.com/ollama/ollama/blob/main/docs/modelfile.md).

## **Automatic Model Updates**

At every startup, **DoxyPatch** checks the hashes of model files in `Models` folder and automatically updates and recreates the model if any changes are detected. This ensures that the model is always up-to-date with the latest prompts and parameters.

## **With-Context Mode**

The `--with-context` mode is currently an **experimental feature**. 

As a result,  demands substantially more GPU RAM capability. If your system lacks sufficient GPU resources, you may encounter performance issues or errors when attempting to utilize this feature.

## **Privacy**

A local model ensures your privacy, keeping all data processing on your machine (so private at 100%).

## **License**

**DoxyPatch** is licensed under the [**GPL (General Public License)**](https://www.gnu.org/licenses/gpl-3.0.en.html). The full text of the license can be found in the `LICENSE` file.

The **GNU General Public License** is a free, copyleft license for software and other kinds of works. It ensures that the software remains free and open, and that any modifications or derivative works are also made available under the same license.

Please note that by using **DoxyPatch**, you agree to comply with the terms and conditions of the **GPL**.

## **Why This Project Matters**

This project serves as an excellent starting point for studying and learning how to modernize old tools by integrating **AI**. **DoxyPatch** demonstrates the process of taking a traditional code documentation tool and enhancing it with **AI-powered capabilities**. By working with small, local models, you can gain valuable experience in **AI integration** and optimization, ensuring that even limited resources can produce high-quality results. This project is particularly interesting for those looking to explore **AI** in practical, real-world applications, providing a solid foundation for more advanced **AI projects** in the future.

## **Contributing**

Contributions are welcome! If you have any ideas or bug fixes, please submit a pull request or issue on this repository.
