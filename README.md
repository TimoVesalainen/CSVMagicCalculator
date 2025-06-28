Used to sort CSV files combining several partial columns

# Requirements

Install Dotnet: https://learn.microsoft.com/en-us/dotnet/core/install/

Compiling requires SDK, running runtime.

Made for .Net 8.0.

# Compiling

Run `dotnet build`.

# Running

 Can be run with `dotnet run -- command line options`, or using executable after compiling. Command line options are:

    -s, --skipFirstLine                    (Default: true) Skip the processing of first line of input file
    -c, --commaChar                        (Default: ,) Comma separator character
    -v, --printValue                       (Default: false) Print the value used for comparison in the output
    --help                                 Display this help screen.
    --version                              Display version information.
    Input (pos. 0)                         Required. Path to input file to read from
    Output (pos. 1)                        Required. Path to output file to write results to
    Primary data column index (pos. 2)     Required. Zero-index position of column of primary data column
    Second data column indexis (pos. 3)    Zero-index position of columns of secondary data columns
