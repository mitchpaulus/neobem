---
title: "Neobem: A domain specific language for generating EnergyPlus input files"
tags:
  - Building energy simulation
  - Domain specific language
  - EnergyPlus
  - Command line
  - DOE-2
  - BDL
  - Building component library

authors:
  - name: Mitchell T. Paulus
    orcid: 0000-0001-6662-8165
    affiliation: "1 2"
    corresponding: true
  - name: Kushan B. Abeyawardhane
    orcid: 0000-0002-1324-5416
    affiliation: "1 2"

affiliations:
  - name: Command Commissioning, LLC
    index: 1
  - name: Texas A&M University, Energy Systems Laboratory, College Station, TX, USA
    index: 2

bibliography: joss.bib
date: 27 August 2022
---

# Summary

Building energy simulation plays a key role in the design and operation of buildings.
An important simulation program is EnergyPlus, developed by the United States Department of Energy [@Crawley2001].
EnergyPlus is used extensively in both academic and industrial settings.

The input for EnergyPlus simulations are simple text files, with data formatted in *Objects*.
*Fields* are separated by commas (`,`), and *Objects* are separated by semicolons (`;`).
For example, an *Object* describing a building:

```idf
Building,
  PSI HOUSE DORM AND OFFICES, !- Name
  36.87,                      !- North Axis {deg}
  Suburbs,                    !- Terrain
  0.04,                       !- Loads Convergence Tolerance Value
  0.4,                        !- Temperature Convergence Tolerance Value {deltaC}
  FullInteriorAndExterior,    !- Solar Distribution
  40,                         !- Maximum Number of Warmup Days
  6;                          !- Minimum Number of Warmup Days
```

Most users will use GUI software such as OpenStudio to create simple building energy models.
However, as with any data entry, a point comes quickly when researchers and developers will want to remove duplication and parameterize models as concisely as possible.

Neobem is a programming language and corresponding compiler designed to meet this need with the following considerations in mind:

1. Neobem files are a superset of the original input files.
3. Expressive syntax designed for the domain.
2. Simplified installation with no dependencies.

# Statement of Need

Neobem focuses on parameterizing the initial model creation expressively and succinctly.
An EnergyPlus IDF (input data file) file is data entry; there is no computation (that is the purpose of the simulation engine which consumes the input).
Virtually any programming language that can write to files can be used to aid in generating this data; however, the ease of use may vary drastically.

In the distribution of EnergyPlus, several auxiliary programs are provided.
One of these programs that aid in input file development is EP-Macro [@EpMacro2021].
It is a console application that does allow for incorporating external files, defining macros with arguments, and limited arithmetic.
It has several major drawbacks that limit its use, including:

 - No ability to loop.
 - Verbose syntax. An example of an `if` statement from the documentation:

   ```
   ##if #[#[** city[ ] EQS Chicago ] and#[** occup[ ] NES low **]** **]**
   ```

 - Difficult to use within a build system. It does not take input on standard input, and file must reside in the same directory as the executable.

Other commonly used software tools and their associated programming ecosystem are:

 - OpenStudio [@Guglielmetti2011], Ruby
 - eplusr [@Jia2021], R
 - Eppy [@eppy], Python

A drawback of all three of these tools is that they each require the setup of the corresponding programming environments.

The intended audience for the software is researchers or practitioners needing complete control over the initial model input, and want the tooling to fit in well within a larger software build system.
The tooling is intended to complement and be used alongside existing software.
The language and tooling were also designed for those with limited programming experience, but who need to be able to do the simple first steps: variable extraction and loops.


# Example Syntax Features

## Inline Data Tables

The required raw data for parameterization can often be organized as a table.
Neobem offers built-in syntax to specify this data directly in the source code in a readable fashion.

The syntax for the inline data table was inspired by the syntax for tables in Markdown, a simple markup language.
The syntax appears like:

```neobem
zones =
________________________________________
'name'        | 'x_origin'  | 'y_origin'
--------------|------------ |-----------
'Bedroom'     | 0           | 0
'Living Room' | 10          | 20
'Kitchen'     | 5           | 12
‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾
```

The result of this expression is a list of dictionaries, with the column headers serving as the dictionary keys.
It is equivalent to the syntax:

```neobem
zones = [
    { 'name': 'Bedroom'    , 'x_origin': 0 , 'y_origin': 0 },
    { 'name': 'Living Room', 'x_origin': 10, 'y_origin': 20 },
    { 'name': 'Kitchen'    , 'x_origin': 5 , 'y_origin': 12 },
]
```

Inline data tables are designed to be used when the number of table cells is relatively small.
There are additional language constructs for similar functionality for loading large datasets from external files such as Microsoft Excel spreadsheets and tab delimited text data.

## Integration with Building Component Library (BCL)

The Building Component Library (BCL) is a collection of data used in creating building energy models [@Fleming2012].
The data is broken up into "components" and "measures". "Components" typically contain data related to physical properties, such as thermal resistance, along with additional meta data.

This data is stored in XML files hosted on [GitHub](https://github.com).

For example, the following component is for a double pane window construction (note some tags removed for brevity).

```xml
<component xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
    xsi:noNamespaceSchemaLocation="component.xsd">
 <schema_version/>
 <name>Dbl Clr 6mm-6mm Air</name>
 <uid>c8cf1c10-c23e-012f-0fe5-00ff10b04504</uid>
 <version_id>9b1dadb4-7bb5-424b-bff2-6e937d4a63a3</version_id>
 <description>Window construction from EnergyPlus
     8.0 WindowConstructs.idf dataset.</description>
 <tags> <tag>Fenestration.Window</tag> </tags>
 <attributes>
     <attribute>
         <name>Overall U-factor</name>
         <value>3.058</value>
         <datatype>float</datatype>
     </attribute>
     <attribute>
         <name>Solar Heat Gain Coefficient</name>
         <value>0.7</value>
         <datatype>float</datatype>
     </attribute>
     <attribute>
         <name>Visible Light Transmittance</name>
         <value>0.781</value>
         <datatype>float</datatype>
     </attribute>
 </attributes>
 <files>
  <file>
   <version>
    <software_program>EnergyPlus</software_program>
    <identifier>8.0.0.008</identifier>
   </version>
   <filename>Dbl Clr 6mm-6mm Air_v8.0.0.008.idf</filename>
   <filetype>idf</filetype>
  </file>
</component>
```

Important pieces of data for an energy model would be the solar heat gain coefficient and the visible light transmittance.

Neobem provides dedicated syntax for including this data in the output.
Each component in the BCL has a Universally Unique Identifier (UUID), a 128 bit number.
The window component above has a id of `c8cf1c10-c23e-012f-0fe5-00ff10b04504`.

The `bcl:<UUID>` syntax loads the component data into a *dictionary*, or an associative array, with all the corresponding attributes available by name.
The attributes are also converted to the corresponding data types, such as floats, strings, or booleans.

```neobem
component = bcl:c8cf1c10-c23e-012f-0fe5-00ff10b04504

WindowMaterial:SimpleGlazingSystem,
  Window,                                    ! Name
  <component.'Overall U-factor'>,            ! U-Factor {W/m2-K}
  <component.'Solar Heat Gain Coefficient'>, ! SHGC
  <component.'Visible Light Transmittance'>; ! Vis. Transmittance
```

Components in the BCL can also optionally provide associated files, including IDF input files.
When loading the BCL component, Neobem also populates the dictionary with key-value pairs,
in which the key is the file type, like `'idf'`{.neobem}, and the value is the URL pointing to the file contents.

In this way, the file can be directly imported using the normal `import`{.nbem} syntax.

```neobem
swinging_door = bcl:2e613270-5ea8-0130-c85b-14109fdf0b37
import swinging_door.'idf'
```

# References
