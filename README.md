# autoPyDoc
An automated language documentation generator for the Python language, written in C#.<br />
<sup>Version 1.0. Published 20180412.1200</sup>

### Description
This is a .NET console application that creates a deep-linking encyclopedic documentation reference from any Python project.

### How Changes to This Project Are Tested
I arbitrarily selected a Python-based project on which to test this application. The one I chose was the complete Python source of the [**pydoit / doit**](https://github.com/pydoit/doit) GitHub project. At the time of this writing, the Python source consists of roughly 129 files, and the output currently expands to 6,678 separate hyperlinked object documentation files.

### Sample Output
You can review the documentation output of the **doit** project [here](http://www.localmarketproductions.com/PyDocSamples/). The Index.html file lists all source files found, and you can drill down from there.

### Coding With the Application
Basic API documentation has been rendered for PyDoc that can be useful in writing code to it. You can find the API file [here](https://danielanywhere.github.io/PyDocDocumentation.html). Other helpful information can be found at my GitHub project page [https://danielanywhere.github.io/](https://danielanywhere.github.io/).

### Using the Application
At this time, a full release hasn't yet been created, but this project can be built to run on Windows, Linux, and macOS. You can either create a command-line console application, or with only slight configuration, it can be used as a shared library, either code-level or runtime. In other words, I am currently waiting to see where user or contributor demand takes the release.

In the meantime, the project can be downloaded and compiled in the Community version of Microsoft Visual Studio to be run as a console application with the following command-line parameters.
```
   PyDoc /inp:{Pathname} /outp:{Pathname} [options]
	     /?                 - Display this message.
       /inp:{Pathname}    - Search within this path for input.
       /outp:{Pathname}   - Write documentation files to this output path.
       /v:{Level}         - Verbosity level. Default=0.
       /w                 - Wait for keypress after application end.
```

### General Notes
Final documentation results depend somewhat on how well you comment your original Python source files, and how descriptively you name all of the functional elements.



