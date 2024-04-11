# Surface Visualizer
This repository contains the source code of a tool that can be used to
display mathematical surfaces, based on the techniques proposed in the paper
"[Exploded View Diagrams of Mathematical Surfaces](http://vecg.cs.ucl.ac.uk/Projects/SmartGeometry/math_exploded_view/paper_docs/math_exploded_view_big.pdf)".

## Compiling

First, ensure that [.NET SDK 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) and [.NET Runtime 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) are installed.

Clone the repository:

`git clone https://github.com/Visne/2IMV10`

Ensure that you are in the SurfaceVisualizer directory (`cd SurfaceVisualizer`) and then
build and run the program:

`dotnet run`

## Pre-built binary

Alternatively, if you are on Windows, you can use the pre-built binary.
First, ensure that [.NET Runtime 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) is installed.
Then download [`binaries.zip`](https://github.com/Visne/2IMV10/raw/main/binaries.zip), and unzip it.
Then the program can be ran by launching `SurfaceVisualizer.exe`.

## Manual
The viewport can be rotated by clicking and dragging.
You can zoom by using the scroll wheel.
To pan up and down, you can hold the middle mouse button and drag, or hold <kbd>Ctrl</kbd> while clicking and dragging.

You can use the panel on the right to select another model (from the built-in library or file system), to change the
display settings, and to where and how the cutting planes are calculated.

Note that when importing a model from the file system, it has to be a triangulated glTF model.
