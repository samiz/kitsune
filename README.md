Kitsune is a programming language based on MIT's Scratch -- a visual educational programming language. The plan is to have a reasonable clone of Scratch then extend it to other directions; the goal is to experiment with tools for casual programming/programming for everyone.

Kitsune is implemented in C# with WinForms. To run it simply clone the repository, open the code in Visual Studio, and run the project.

Currently implemented:
* Turtle graphics
* Some basic control flow (repeat, repeat forever, wait...)
* Most typical graphical editing actions (move things around, snap to join, drag to separate)
* A cute fox sprite/mascot :)

Needs to be done
* Concurrency
* Supporting multiple sprites
* Variables

The implementation now is in a state where a user can toy with simple geometric programs.

Unlike Scratch, which seems to interpret the program as an AST, Kitsune compiles the graphical scripts to an in-memory intermediate representation that's then run by a simple VM. This IR-based design might be useful in later stages for compiling programs to other platforms, like the JVM or JavaScript.