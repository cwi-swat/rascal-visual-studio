# rascal-visual-studio

This project is an experiment in making Rascal code analyzers and transformers directly available in Microsoft Visual Studio via
a reusable extension.

It is good to realize that this is not the same thing as the Visual Studio Code extension: usethesource/rascal-language-servers

The minimal viable product is:
* loads Rascal code into a JVM
* calls a given Rascal function on demand from Visual Studio
  * from a menu option
  * after "save" in a file with a certain extension
  * the function receives a file name as a parameter
  * possibly other parameters such as current selection?

