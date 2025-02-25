# Tutorials

Instead of starting with the boring details about installation and
reference material, let's jump right into the good stuff. This will
focus on the basics, giving you a general understanding of what this
tool is all about, without worrying too much about the details.[^tikz]

[^tikz]: Inspiration from the TikZ-PGF manual.

## Tutorial 1: Variables

You are building your EnergyPlus input file using conventional tools,
and you end up with objects like so:

```nbem
Zone,
  Atrium,        ! Name
  0,             ! Direction of Relative North {deg}
  0,             ! X Origin {m}
  0,             ! Y Origin {m}
  0,             ! Z Origin {m}
  1,             ! Type
  1,             ! Multiplier
  autocalculate, ! Ceiling Height {m}
  autocalculate, ! Volume {m3}
  autocalculate, ! Floor Area {m2}
  ,              ! Zone Inside Convection Algorithm
  ,              ! Zone Outside Convection Algorithm
  Yes;           ! Part of Total Floor Area


Schedule:Constant,
  Atrium Schedule, ! Name
  Atrium Limits,   ! Schedule Type Limits Name
  1;               ! Hourly Value
```

Do you see all those `Atrium`s? If you're a programmer, you know those
can be abstracted out to a variable. If you ever want to change the
name, you can easily do it in one place. How do you do it? Like so:

```nbem
INCLUDE code_samples/variable_sample.nbem
```

A few pieces of syntax to see here:

1. Variable declaration

    `var_name = <expression>`

   One thing to note about the variable name. It *must* begin with a
   lowercase alphabetical character. This is to help differentiate
   between variable declarations and regular Object declarations.

2. Expression placeholders

    `< <expression> >`


   In our Object declaration, we can insert expressions anywhere by
   putting them in angle brackets '`< >`'. If you really need to have a
   angle bracket, you can escape them by doubling them up, like '`<<`'.


## Tutorial 2: Functions

Say you've continued to build your input file, and you end up with the
six following objects:

```nbem
Schedule:Constant,
  Zone Space Temperature Setpoint Schedule, ! Name
  Zone Space Temperature Limits,            ! Schedule Type Limits Name
  22;                                       ! Hourly Value

Schedule:Constant,
  Supply Air Temp Setpoint Schedule, ! Name
  Supply Air Temp Limits,            ! Schedule Type Limits Name
  13;                                ! Hourly Value

Schedule:Constant,
  CHW Supply Temp Setpoint Schedule, ! Name
  CHW Supply Temp Limits,            ! Schedule Type Limits Name
  7;                                 ! Hourly Value

ScheduleTypeLimits,
  Zone Space Temperature Limits, ! Name
  20,                            ! Lower Limit Value
  24,                            ! Upper Limit Value
  Continuous,                    ! Numeric Type [Continuous, Discrete]
  Temperature;                   ! Unit Type

ScheduleTypeLimits,
  Supply Air Temp Limits, ! Name
  10,                     ! Lower Limit Value
  16,                     ! Upper Limit Value
  Continuous,             ! Numeric Type [Continuous, Discrete]
  Temperature;            ! Unit Type

ScheduleTypeLimits,
  CHW Supply Temp Limits, ! Name
  5,                      ! Lower Limit Value
  10,                     ! Upper Limit Value
  Continuous,             ! Numeric Type [Continuous, Discrete]
  Temperature;            ! Unit Type

```

Can you see the repetition here? Each schedule has an associated
schedule limits, in which the names match. We can make this situation
better by introducing a *function*.

```nbem
INCLUDE code_samples/const_schedule_example.nbem
```

This will produce the exact same objects as before. But we now can trust
that our names will be correct, we went from 35 lines to 17, and adding
a new constant schedule like this is just one more line.

Let's discuss what we've seen here - Here's the official grammar for
defining a function (named `lambda_def` in the grammar):

```antlr
lambda_def : ('\\' | 'λ' ) IDENTIFIER* '{' (expression | function_statement*) '}' ;
```

In words, a lambda definition is a backslash character (or $\lambda$ symbol),
followed by 0 or more identifiers, with a single expression *or* one or
more function statements in curly braces.

So technically, in our example with the schedule, this is a *variable
declaration* where the expression on the right hand side being assigned
is a *function expression*.  Pretty much everything in Neobem is an
*expression*.  And in Neobem, functions are first class. They can be
assigned to variables and be returned by other functions.

This means you can build higher order functions like:

```nbem
INCLUDE code_samples/higher_order_function.nbem
```

The same thing could even be written on a single line like:

```nbem
INCLUDE code_samples/single_line_higher_order_function.nbem
```

The result of this is `7`. See, functions can return other functions, and
they have the concept of "closures". That is, functions can see all the
defined variables in the above scopes. For example, in the inner defined
function `\y { x + y }`, it knows of the `x` from the surrounding
function.

The second example showed how you don't even need to necessarily name
your function if you want. You can define anonymous functions wherever
you like.

This example showed the one of function forms, where inside the curly
braces is a single expression. However, inside a function, you can also
provide one or more *function statements*. This is actually what you saw
in the example with the schedules.

A function statement can be:

1. An object declaration
2. A variable declaration
3. An input file comment
4. A return statement

Neobem can be considered a mostly *functional* language. In a truly
functional programming language, there *are no side-effects*. However,
for practical reasons, the Neobem language has one important built in
side-effect. That is the object declaration.

When the compiler encounters an object declaration from inside or
outside of a function, it will print it out to the final result, making
any required replacements (The same is true for input file comments).
*This is the only way objects are printed out*.

This is how the concept of what some might think of as a "template" and
a mathematical function are the same thing in Neobem. These are both
functions to Neobem. One has a side effect of printing to the result,
and the other just computes a value.

```nbem
INCLUDE code_samples/template_function_example.nbem
```

Beside object declarations and input file comments, the other two
possible statements are the variable declaration and the return
statement.

A variable declaration is the same declaration introduced earlier.
However, if a variable is declared within a function, it is not visible
from outside that function. It is normally used to break up a
complicated expression that make be easier to understand in more steps.

The return statement sets the resulting expression that is returned by
the function. An example of variable declaration and the return
statement would be:

```nbem
INCLUDE code_samples/hard_computation.nbem
```

## Tutorial 3: Utilizing Tabular Data, Dictionaries, and Lists

The goal with a domain specific language like this is to reduce the
amount of syntax required to express our intent. At the extreme, the
minimum we must provide is the *data*. So a novel feature of the
Neobem language is the integration of data entry directly into the
language.

Lets say I have tabular data about my zones and want to create the
required objects for them. An example is the easiest way to show how this
would be done in Neobem.

```nbem
INCLUDE code_samples/inline_data_table_example.nbem
```

Woah - there's a lot going on here. Let's unpack this. There are 3
lines, the first two are variable declarations, followed by a print
statement.

The first variable declaration

```nbem
zones =
________________________________________
'name'        | 'x_origin'  | 'y_origin'
--------------|------------ |-----------
'Bedroom'     | 0           | 0
'Living Room' | 10          | 20
'Kitchen'     | 5           | 12
________________________________________
```

shows an *inline data table expression*. It looks like a pretty table,
but it is actually an expression that evaluates to a *list* of
*dictionaries*. The borders can be made with hyphens `'-'` and pipe
characters `'|'`

A list expression is what you would expect. It's a list of expressions.
This is like an "array" in other programming languages. You can define
your own lists anywhere an expression is expected (because it is an
expression) using square brackets `'['` and `']'` with items separated
by commas.

Example:

```nbem
my_list = [ 1, 2, 'a string!', sin(3.1415926), \x { x + 1 } ]
```

Notice that you can put whatever expression *types* you want into a single
list. The example has 2 numeric expressions, a string expression, a
numeric expression that is the result of function application, and a
function expression.

A *dictionary* is a way to group expressions into a single identifier.
This is the concept of an ["associative
array"](https://en.wikipedia.org/wiki/Associative_array) implemented as
a ["hash table"](https://en.wikipedia.org/wiki/Hash_table) with a string
key, the same concept as in many other programming languages.
Unfortunately for me, I cannot name this an "object" since the concept
of an object is already taken by the "objects" in the resulting building
energy simulation files we are trying to create.

You define a dictionary like this:

```nbem
INCLUDE code_samples/dictionary_example.nbem
```

You can access the contents of the dictionary using the member access
operator, a period `.`, followed by the string key.

So for our dictionary example:

```nbem
my_dictionary.'prop2' == 'Some text'
my_dictionary.'prop_3'.'nested_dictionary' == 'I am nested'
```

Note the key used in the member access can be *any* string. For example,
you can use pretty much any Unicode characters you'd like. Using music
notes is totally valid:

```neobem
dictionary = {
    '♬': 9.4
}

Version,
    < dictionary.'♬' >;
```

And the key doesn't have to be a *string literal*, it just has to
*evaluate* to a string. So this is no problem either:

```neobem
INCLUDE code_samples/variable_key.nbem
```

In fact, in the original definition, they key doesn't even have to be a
string literal, it can be any arbitrary expression that evaluates to a
string. If you were feeling wild, you could do something like:

```neobem
INCLUDE code_samples/wild_example.nbem
```

That does evaluate to `Version,9.4;`{.nbem}, give it a try!

Now that we know what a list and dictionary are, what is the result of
that inline data table? It's a *list of dictionaries*.

That means these are exactly the same.

```neobem
zones =
________________________________________
'name'        | 'x_origin' | 'y_origin'
--------------|------------|------------
'Bedroom'     | 0          | 0
'Living Room' | 10         | 20
'Kitchen'     | 5          | 12
________________________________________

zones = [
    {
        'name': 'Bedroom',
        'x_origin': 0,
        'y_origin': 0
    },
    {
        'name': 'Living Room',
        'x_origin': 10,
        'y_origin': 20
    },
    {
        'name': 'Kitchen',
        'x_origin': 5,
        'y_origin': 12
    }
]
```

Why would you use one version over another? I would generally prefer the
inline data table for situations when you have data *that doesn't
include nested dictionaries*. If nested dictionaries are required, then
you'd have to use the normal dictionary syntax.

That covers the first variable declaration of our example file.

The second variable declaration

```neobem
zone_template = λ zone {
Zone,
  <zone.'name'>,     ! Name
  0,                 ! Direction of Relative North {deg}
  <zone.'x_origin'>, ! X Origin {m}
  <zone.'y_origin'>, ! Y Origin {m}
  0,                 ! Z Origin {m}
  1,                 ! Type
  1,                 ! Multiplier
  autocalculate,     ! Ceiling Height {m}
  autocalculate,     ! Volume {m3}
  autocalculate,     ! Floor Area {m2}
  ,                  ! Zone Inside Convection Algorithm
  ,                  ! Zone Outside Convection Algorithm
  Yes;               ! Part of Total Floor Area
}

```

is a normal function with an object declaration. The only unique thing
about it is that the input parameter is expected to be a dictionary that
has 3 keys: `name`, `x_origin`, and `y_origin`.

Did you also notice that lambda (`λ`) character in the function
definition? It's allowed instead of the boring `\` character if you'd
like. Why the lambda character? If you don't know why, search for
[lambda calculus](https://en.wikipedia.org/wiki/Lambda_calculus) and
dive into that rabbit hole.

The final statement is

```nbem
print map(zones, zone_template)
```

This is the evaluation of the built in `map` function, with the function
`zone_template` and list of dictionaries `zones` as inputs.

The `map` function is a extremely important function in functional
programming languages. It allows you to "map" or evaluate a function
over each element of a list. The resulting expression is a new list,
with each list item being transformed by the function.

However, in Neobem remember, functions can have one important
side-effect: printing out objects.

So the output to our compiled idf file from the `map` function is:

```nbem
INCLUDE code_samples/inline_data_table_example.output.idf
```

I hope you can take in how expressive that is. Write the minimal amount
of data, then map a template function over it to build all your objects.

## Tutorial 4: Introducing Logic

If we want to have a Turing complete programming language, we need
branching ability - which we accomplish in Neobem using *if
expressions*.

Notice how I called it an *expression*. Like everything else, our
if-else ability is an expression, meaning it can always be reduced to a
new expression.

The syntax for the if expression is

```nbem
if <expression> then <expression> else <expression>
```

The `else`{.nbem} is required because there must always be a result.

For this tutorial, let's use logic to determine which template we want to
use for a given fan.

```nbem
INCLUDE code_samples/fan_logic_sample.nbem
```

Notice here how we used an equality operator (`==`) to determine what function
to use.

We also introduced some helper functions that do some conversion from IP
to SI units. The units for EnergyPlus input files are base SI. In many
cases this is not convenient. We can wrap values in functions, so that
our entry is in the units we want, but the output correctly matches the
energy simulation requirements.


## Tutorial 5: Importing From Other Files

Large software projects are not built in one monolithic file. Engineers
break the code up into smaller pieces so that it is more manageable.
Building simulation files can get quite large, thousands of lines of
code, so it makes sense that we should be able to do the same thing in
Neobem.

A small, but typical example of breaking out the input file in multiple
files could be this.

**in.nbem**
```nbem
INCLUDE code_samples/in_ver_1.nbem
```

where in the same directory as the `in.nbem` file is the two files

1. `defaults.nbem`
2. `chillers.nbem`

If the contents of those are

**defaults.nbem**
```nbem
INCLUDE code_samples/defaults.nbem
```

**chillers.nbem**
```nbem
INCLUDE code_samples/chillers.nbem
```

then the resulting output would be:

```nbem
INCLUDE code_samples/output_include_example.idf
```

Neat, right? So perhaps the simplest way that Neobem can help you with
your current files is by breaking them up into smaller pieces.

To note here:

- The string is a relative file path to the file to import.
- Notice the last line of the file `defaults.nbem`. It is an *export*
  statement, and it is required for other files to use the variables and
  functions that are defined.

  However, if the imported script generates any output, that is passed
  along no matter what - no `export` is required. This is what happens
  in the `chillers.nbem` file.

- By default, exported items will overwrite existing identifiers with
  the same name. You can get around this by *qualifying* the import
  using the `as` option.

  Using the previous example, we could have done:

  **in.nbem**
  ```nbem
  INCLUDE code_samples/in_qualified.nbem
  ```

  The `as` option will put whatever string is specified as a prefix to
  all exports from the file, with an `@` sign between.

## Tutorial 6: Using the Building Component Library

The [Building Component Library](https://bcl.nrel.gov/) (BCL) is a library of "components" and "measures" to be used in energy models.
It is hosted by NREL in conjunction with other National Laboratories from the United States.
The data is hosted in GitHub repositories, searchable through a straightforward API.

Components contain the raw data for building up an energy model. Straight from the BCL,
here is how they are described:

> The BCL contains components which are the building blocks of an energy model.
> They can represent physical characteristics of the building such as roofs,
> walls, and windows, or can refer to related operational information such as occupancy and equipment schedules and weather information.
> Each component is identified through a set of attributes that are specific to its type,
> as well as other metadata such as provenance information and associated files.

As the BCL is currently the de facto standard hosting entity for this type of information, it seemed pragmatic to dedicate
*Neobem programming language syntax* to it.

### Using BCL Attributes

For example, let's imagine we are trying to model the loads in our zone, and we have a Mr. Coffee coffee maker.
Using the BCL, we search for components with the tag `Appliance.Coffee Maker`.

I see the component exists, and I download it to check it out.

![BCL search results for coffee makers](img_neobem/bcl_search.png){width=80%}

If I download that component, it will be a compressed zip file with a single directory and a file called `component.xml`.
The `xml` file contains our data to be used in the energy model.

Here's the contents of the `component.xml` file for our coffee maker:

```xml
<component xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
           xsi:noNamespaceSchemaLocation="component.xsd">
 <schema_version/>
 <name>Mr. Coffee -VBX20</name>
 <uid>b87b9630-c2c7-012f-13bc-00ff10b04504</uid>
 <version_id>664faf5e-a91b-4b06-bc73-0c79c39c9afb</version_id>
 <xml_checksum/>
 <display_name/>
 <description/>
 <modeler_description/>
 <source>
  <manufacturer>Mr. Coffee</manufacturer>
  <model>VBX20</model>
  <serial_no/>
  <year>2012</year>
  <url>http://www.mrcoffee.com/Manuals/MANUALS/VBX20_43_65900646.PDF </url>
 </source>
 <tags>
  <tag>Appliance.Coffee Maker</tag>
 </tags>
 <attributes>
  <attribute>
   <name>Peak Power</name>
   <value>830.0</value>
   <datatype>float</datatype>
  </attribute>
 </attributes>
</component>
```

Notice there are lots of fields and data available. We have a description of the manufacturer, URL reference, and a peak power consumption of 830 W.

Also notice that every component in the BCL has a [*universally unique identifier (UUID)*](https://en.wikipedia.org/wiki/Universally_unique_identifier),
a 128 bit id that as you might expect, *uniquely identifies it*.

So we want to inject this data into our energy model. We can do that using the UUID.

Let's make an `ElectricEquipment` object to represent the heat gain from the coffee maker.

```neobem
INCLUDE code_samples/coffee_maker.nbem
```

The idf output from this is:

```neobem
INCLUDE code_samples/coffee_maker.output.idf
```

The line

```neobem
coffee_maker = bcl:b87b9630-c2c7-012f-13bc-00ff10b04504
```

sets the variable `coffee_maker` to a dictionary representing that component.
It will match the schema found in the XML file.
It also lifts all the `attributes` to the root of the dictionary.
That's why the `<coffee_maker.'Peak Power'>`{.neobem} expression works.
Otherwise you'd have to something extremely verbose to extract out the attribute, something like:

```neobem
wattage = (coffee_maker.'attributes'.'attribute'
            |> λ attr { attr.'name' == 'Peak Power' }
            -> index(0)).'value'
```

and nobody wants to do stuff like that.


### Using BCL Files

For some components, idf objects will already be defined in an attached idf file.
This is quite useful, as we can then simply import that file.

For example, let's look at the component with the UUID `2e613270-5ea8-0130-c85b-14109fdf0b37`,
a non residential swinging door meeting ASHRAE Standard 90.1-2007 requirements for climate zone 1A.

If you download that component you'll see the `component.xml` file as before, but now there is a directory called `files`.

The contents looks like:

![Door component contents](img_neobem/bcl_component_directory_structure.png){width=80%}

There are files for OpenStudio (`.osc` and `.osm`) and EnergyPlus (`.idf`).

When the component is loaded as a dictionary in Neobem, it will populate the URL to these files.

This makes including the file straightforward. An example would look like this:

```neobem
INCLUDE code_samples/include_door.nbem
```

Remember that the `import`{.neobem} statement in Neobem is simple and explicit.
You give it a string URI, and Neobem downloads it as a Neobem file, executes it, and includes any idf output in the final result.

The expression `bcl:2e613270-5ea8-0130-c85b-14109fdf0b37` is a dictionary.
We are then getting the value for the key `idf`.
That key is a long string with the with the value of a URL (Breaking it up onto several lines so as to not ruin the documentation layout).

```neobem
'https://bcl.nrel.gov/api/file?' +
'path=BuildingComponentLibrary-nrel-components/' +
'v0.3/90.1-2007 Nonres 1A Door Swinging/files/' +
'90.1-2007 Nonres 1A Door Swinging_v7.1.0.idf'
```

You can try out that endpoint directly in your browser by clicking the link
[here](https://bcl.nrel.gov/api/file?path=BuildingComponentLibrary-nrel-components/v0.3/90.1-2007 Nonres 1A Door Swinging/files/90.1-2007 Nonres 1A Door Swinging_v7.1.0.idf).

You could have manually put this URL in the `import`{.neobem} statement like:

```neobem
import 'https://bcl.nrel.gov/api/file?' +
       'path=BuildingComponentLibrary-nrel-components/' +
       'v0.3/90.1-2007 Nonres 1A Door Swinging/files/' +
       '90.1-2007 Nonres 1A Door Swinging_v7.1.0.idf'
```

but using the UUID and the '`idf`' key expresses the intent more clearly and with fewer characters.

This will download the associated idf file, which has the contents:

```idf
INCLUDE code_samples/include_door.output.idf
```

**Note**: In some of the idf files provided in the BCL, a '`Version`' object is supplied.
If multiple idf files were included, you would run into an EnergyPlus error for providing multiple
'`Version`' objects. Because of this, Neobem will automatically remove any '`Version`' objects from the
downloaded idf file.

<!-- Force break between sections -->
