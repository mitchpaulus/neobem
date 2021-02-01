## Tutorial 1: Variables

You are building your EnergyPlus input file using conventional tools,
and you end up with objects like so:

```bemp
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

```bemp

atrium_name = 'Atrium'

Zone,
  { atrium_name }, ! Name
  0,               ! Direction of Relative North {deg}
  0,               ! X Origin {m}
  0,               ! Y Origin {m}
  0,               ! Z Origin {m}
  1,               ! Type
  1,               ! Multiplier
  autocalculate,   ! Ceiling Height {m}
  autocalculate,   ! Volume {m3}
  autocalculate,   ! Floor Area {m2}
  ,                ! Zone Inside Convection Algorithm
  ,                ! Zone Outside Convection Algorithm
  Yes;             ! Part of Total Floor Area


Schedule:Constant,
  { atrium_name } Schedule, ! Name
  { atrium_name } Limits,   ! Schedule Type Limits Name
  1;                        ! Hourly Value

```

A few pieces of syntax to see here:

1. Variable declaration

    `var_name = <expression>`

   One thing to note about the variable name. It *must* begin with a
   lowercase alphabetical character. This is to help differentiate
   between variable declarations and regular Object declarations.

2. Expression placeholders

    `{ <expression> }`


   In our Object declaration, we can insert expressions anywhere by
   putting them in curly braces '`{ }`'. If you really need to have a
   curly brace, you can escape them by doubling them up, like '`{{`'.


## Tutorial 2: Functions

Say you've continued to build your input file, and you end up with the
six following objects:

```bemp
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

```bemp
const_temp_schedule = \ name, value, lower, upper {
Schedule:Constant,
  { name } Schedule, ! Name
  { name } Limits,   ! Schedule Type Limits Name
  { value };         ! Hourly Value

ScheduleTypeLimits,
  { name } Limits, ! Name
  { lower },       ! Lower Limit Value
  { upper },       ! Upper Limit Value
  Continuous,      ! Numeric Type [Continuous, Discrete]
  Temperature;     ! Unit Type
}

print const_temp_schedule('Zone Space Temperature', 22, 20, 24)
print const_temp_schedule('Supply Air Temp'       , 13, 10, 16)
print const_temp_schedule('CHW Supply Temp'       , 7 , 5 , 10)
```

This will produce the exact same objects as before. But we now can trust
that our names will be correct, we went from 35 lines to 17, and added a
new constant schedule like this is just one more line.

Let's discuss what we've seen here - Here's the official grammar for
defining a function (named `lambda_def` in the grammar):

```antlr
lambda_def : ('\\' | 'λ' ) IDENTIFIER* '{' (expression | function_statement*) '}' ;
```

In words, a lambda definition is a backslash character, followed by 0 or
more identifiers, with a single expression or one or more function
statements in curly braces.

So technically, in our example with the schedule, this is a *variable
declaration* where the expression on the right hand side being assigned
is a *function expression*.  Pretty much everything in `bemp` is an
*expression*.  And in `bemp`, functions are first class. They can be
assigned to variables and be returned by other functions.

This means you can build higher order functions like:

```bemp
my_add = \ x { \y { x + y }}

add_two = my_add(2)

print add_two(5)
```

The same thing could even be written on a single line like:

```bemp
print (\x { \y { x + y }})(2)(5)
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

`bemp` can be considered a mostly *functional* language. In a truly
functional programming language, there *are no side-effects*. However,
for practical reasons, the `bemp` language has one important built in
side-effect. That is the object declaration.

When the compiler encounters an object declaration from inside or
outside of a function, it will print it out to the final result, making
any required replacements (The same is true for input file comments).
*This is the only way objects are printed out*.

This is how the concept of what some might think of as a "template" and
a mathematical function are the same thing in `bemp`. These are both
functions to `bemp`. One has a side effect of printing to the result,
and the other just computes a value.

```bemp
version_template = \ ver {
Version,
    { ver };
}

math_function = \ x { (x + 2)^3 }
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

```bemp
my_hard_computation = \ input {
    numerator   = sin(input * 2) + ceiling(input)
    denominator = log(input)
    return numerator / denominator
}
```

## Tutorial 3: Utilizing Tabular Data, Structures, and Lists

The goal with a domain specific language like this is to reduce the
amount of syntax required to express our intent. At the extreme, the
minimum we must provide is the *data*. So a novel feature of the
`bemp` language is the integration of data entry directly into the
language.

Lets say I have tabular data about my zones and want to create the
required objects for them. An example is the easiest way to show how this
would be done in `bemp`.

```bemp
zones =
________________________________________
name          | x_origin    | y_origin
--------------|------------ |-----------
'Bedroom'     | 0           | 0
'Living Room' | 10          | 20
'Kitchen'     | 5           | 12
________________________________________

zone_template = λ zone {
Zone,
  { zone.name },     ! Name
  0,                 ! Direction of Relative North {deg}
  { zone.x_origin }, ! X Origin {m}
  { zone.y_origin }, ! Y Origin {m}
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

print map(zone_template, zones)
```

Woah - there's a lot going on here. Let's unpack this. There are 3
lines, the first two are variable declarations, followed by a print
statement.

The first variable declaration

```bemp
zones =
________________________________________
name          | x_origin    | y_origin
--------------|------------ |-----------
'Bedroom'     | 0           | 0
'Living Room' | 10          | 20
'Kitchen'     | 5           | 12
________________________________________
```

shows an *inline data table expression*. It looks like a pretty table,
but it is actually an expression that evaluates to a *list* of
*structures*.

A list expression is what you would expect. It's a list of expressions.
This is like an "array" in other programming languages. You can define
your own lists anywhere an expression is expected (because it is an
expression) using square brackets `'['` and `']'` with items separated
by commas.

Example:

```bemp
my_list = [ 1, 2, 'a string!', sin(3.1415926), \x { x + 1 } ]
```

Notice that you can put whatever expression types you want into a single
list. The example has 2 numeric expressions, a string expression, a
numeric expression that is the result of function application, and a
function expression.

A *structure* is a way to group expressions into a single identifier.
This is the same concept as an "object" or "class" in many other
programming languages. Unfortunately for me, I cannot name this an
"object" since the concept of an object is already taken by the
"objects" in the resulting file.

You define a structure like this:

```bemp
my_struct = {
    prop_1 : 42,
    prop_2 : 'Some text'
    prop_3 : {
        nested_struct_prop : 'I am nested'
    }
    prop_4 : [ 1, 2, 3]
}
```

You can access the contents of the structure using the member access
operator, a period `.`.

So for our structure example:

```bemp
my_struct.prop2 == 'Some text'
my_struct.prop_3.nested_struct_prop == 'I am nested'
```

Now that we know what a list and structure are, what is the result of
that inline data table? It's a *list of structures*.

That means these are exactly the same.

```bemp
zones =
________________________________________
name          | x_origin    | y_origin
--------------|------------ |-----------
'Bedroom'     | 0           | 0
'Living Room' | 10          | 20
'Kitchen'     | 5           | 12
________________________________________

zones = [
    {
        name: 'Bedroom',
        x_origin: 0,
        y_origin: 0
    },
    {
        name: 'Living Room',
        x_origin: 10,
        y_origin: 20
    },
    {
        name: 'Kitchen',
        x_origin: 5,
        y_origin: 12
    }
]
```

Why would you use one version over another? I would generally prefer the
inline data table for situations when you have data *that doesn't
include nested structures*. If nested structures are required, then
you'd have to use the normal structure syntax.

That covers the first variable declaration of our example file.

The second variable declaration

```bemp
zone_template = λ zone {
Zone,
  { zone.name },     ! Name
  0,                 ! Direction of Relative North {deg}
  { zone.x_origin }, ! X Origin {m}
  { zone.y_origin }, ! Y Origin {m}
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
about it is that the input parameter is expected to be a structure that
has 3 properties: `name`, `x_origin`, and `y_origin`.

Did you also notice that lambda (`λ`) character in the function
definition? It's allowed instead of the boring `\` character if you'd
like. Why the lambda character? If you don't know why, search for
[lambda calculus](https://en.wikipedia.org/wiki/Lambda_calculus) and
dive into that rabbit hole.

The final statement is

```bemp
print map(zone_template, zones)
```

This is the evaluation of the built in `map` function, with the function
`zone_template` and list of structures `zones` as inputs.

The `map` function is a extremely important function in functional
programming languages. It allows you to "map" or evaluate a function
over each element of a list. The resulting expression is a new list,
with each list item being transformed by the function.

However, in `bemp` remember, functions can have one important
side-effect: printing out objects.

So the output to our compiled idf file from the `map` function is:

```bemp
Zone,
  Bedroom,       ! Name
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


Zone,
  Living Room,   ! Name
  0,             ! Direction of Relative North {deg}
  10,            ! X Origin {m}
  20,            ! Y Origin {m}
  0,             ! Z Origin {m}
  1,             ! Type
  1,             ! Multiplier
  autocalculate, ! Ceiling Height {m}
  autocalculate, ! Volume {m3}
  autocalculate, ! Floor Area {m2}
  ,              ! Zone Inside Convection Algorithm
  ,              ! Zone Outside Convection Algorithm
  Yes;           ! Part of Total Floor Area

Zone,
  Kitchen,       ! Name
  0,             ! Direction of Relative North {deg}
  5,             ! X Origin {m}
  12,            ! Y Origin {m}
  0,             ! Z Origin {m}
  1,             ! Type
  1,             ! Multiplier
  autocalculate, ! Ceiling Height {m}
  autocalculate, ! Volume {m3}
  autocalculate, ! Floor Area {m2}
  ,              ! Zone Inside Convection Algorithm
  ,              ! Zone Outside Convection Algorithm
  Yes;           ! Part of Total Floor Area
```

I hope you can take in how expressive that is. Write the minimal amount
of data, then map a template function over it to build all your objects.

## Tutorial 4: Introducing Logic

If we want to have a Turing complete programming language, we need
branching ability - which we accomplish in `bemp` using *if
expressions*.

Notice how I called it an *expression*. Like everything else, our
if-else ability is an expression, meaning it can always be reduced to a
new expression.

The syntax for the if expression is

```bemp
if <expression> then <expression> else <expression>
```

The `else` is required because there must always be a result.

For this tutorial, let's use logic to determine which template we want to
use for a given fan.

```bemp
in_h2o_2_pa = \ in_h2o { in_h2o * 249.08891 }
cfm_2_m3s = \ cfm { cfm / 2118.88 }

variable_fan = \name, cfm, press {
Fan:VariableVolume,
  { name },               ! Name
  ,                       ! Availability Schedule Name
  0.7,                    ! Fan Total Efficiency
  { in_h2o_2_pa(press) }, ! Pressure Rise {Pa}
  { cfm_2_m3s(cfm) },     ! Maximum Flow Rate {m3/s}
  Fraction,               ! Fan Power Minimum Flow Rate Input Method
  0.25,                   ! Fan Power Minimum Flow Fraction
  ,                       ! Fan Power Minimum Air Flow Rate {m3/s}
  0.9,                    ! Motor Efficiency
  1.0,                    ! Motor In Airstream Fraction
  0,                      ! Fan Power Coefficient 1
  0,                      ! Fan Power Coefficient 2
  1,                      ! Fan Power Coefficient 3
  0,                      ! Fan Power Coefficient 4
  0,                      ! Fan Power Coefficient 5
  { name } Inlet,         ! Air Inlet Node Name
  { name } Outlet,        ! Air Outlet Node Name
  General;                ! End-Use Subcategory
}

constant_fan = \name, cfm, press {
Fan:ConstantVolume,
  { name },               ! Name
  ,                       ! Availability Schedule Name
  0.7,                    ! Fan Total Efficiency
  { in_h2o_2_pa(press) }, ! Pressure Rise {Pa}
  { cfm_2_m3s(cfm) },     ! Maximum Flow Rate {m3/s}
  0.9,                    ! Motor Efficiency
  1.0,                    ! Motor In Airstream Fraction
  { name } Inlet,         ! Air Inlet Node Name
  { name } Outlet,        ! Air Outlet Node Name
  General;                ! End-Use Subcategory
}


fans = [
    {
        name: 'Fan 1',
        type: 0,
        press: 5,
        cfm: 10000
    },
    {
        name: 'Fan 2',
        type: 1,
        press: 6,
        cfm: 20000
    },
]

which_fan = \fan {
if fan.type == 0 then
    constant_fan(fan.name, fan.press, fan.cfm)
else
    variable_fan(fan.name, fan.press, fan.cfm)
}

print map(which_fan, fans)

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
`bemp`.

A small, but typical example of breaking out the input file in multiple
files could be this.

**in.bemp**
```bemp
import 'defaults.bemp'

print simulation_params()

import 'chillers.bemp'
```

where in the same directory as the `in.bemp` file is the two files

1. `defaults.bemp`
2. `chillers.bemp`

If the contents of those are

**defaults.bemp**
```bemp
simulation_params = λ {
Version,9.4;

Timestep,6;

ZoneAirHeatBalanceAlgorithm,EulerMethod;
}
```

**chillers.bemp**
```bemp
chiller = λ unit_number {

chiller_name = 'Chiller ' + unit_number

tons_to_watts = λ tons { tons * 3516.8528 }

Chiller:ConstantCOP,
  { chiller_name },            ! Name RefList: [Chillers, validPlantEquipmentNames, validBranchEquipmentNames], RefClassList: [validPlantEquipmentTypes, validBranchEquipmentTypes], REQ, #1
  { tons_to_watts(1000) },     ! Nominal Capacity {W}, AS, REQ, #2
  6,                           ! Nominal COP {W/W}, REQ, #3
  autosize,                    ! Design Chilled Water Flow Rate {m3/s}, AS, #4
  autosize,                    ! Design Condenser Water Flow Rate {m3/s}, AS, #5
  { chiller_name } CHW Inlet,  ! Chilled Water Inlet Node Name REQ, #6
  { chiller_name } CHW Outlet, ! Chilled Water Outlet Node Name REQ, #7
  { chiller_name } CW Inlet,   ! Condenser Inlet Node Name #8
  { chiller_name } CW Outlet,  ! Condenser Outlet Node Name #9
  AirCooled,                   ! Condenser Type Def: AirCooled, [AirCooled, WaterCooled, EvaporativelyCooled], #10
  NotModulated,                ! Chiller Flow Mode Def: NotModulated, [ConstantFlow, LeavingSetpointModulated, NotModulated], #11
  1.0;                         ! Sizing Factor Def: 1.0, #12
}

print map(chiller, [1, 2, 3])
```

then the resulting output would be:


```bemp
Version,9.4;

Timestep,6;

ZoneAirHeatBalanceAlgorithm,EulerMethod;

Chiller:ConstantCOP,
  Chiller 1,            ! Name RefList: [Chillers, validPlantEquipmentNames, validBranchEquipmentNames], RefClassList: [validPlantEquipmentTypes, validBranchEquipmentTypes], REQ, #1
  3516852.8,            ! Nominal Capacity {W}, AS, REQ, #2
  6,                    ! Nominal COP {W/W}, REQ, #3
  autosize,             ! Design Chilled Water Flow Rate {m3/s}, AS, #4
  autosize,             ! Design Condenser Water Flow Rate {m3/s}, AS, #5
  Chiller 1 CHW Inlet,  ! Chilled Water Inlet Node Name REQ, #6
  Chiller 1 CHW Outlet, ! Chilled Water Outlet Node Name REQ, #7
  Chiller 1 CW Inlet,   ! Condenser Inlet Node Name #8
  Chiller 1 CW Outlet,  ! Condenser Outlet Node Name #9
  AirCooled,            ! Condenser Type Def: AirCooled, [AirCooled, WaterCooled, EvaporativelyCooled], #10
  NotModulated,         ! Chiller Flow Mode Def: NotModulated, [ConstantFlow, LeavingSetpointModulated, NotModulated], #11
  1.0;                  ! Sizing Factor Def: 1.0, #12

Chiller:ConstantCOP,
  Chiller 2,            ! Name RefList: [Chillers, validPlantEquipmentNames, validBranchEquipmentNames], RefClassList: [validPlantEquipmentTypes, validBranchEquipmentTypes], REQ, #1
  3516852.8,            ! Nominal Capacity {W}, AS, REQ, #2
  6,                    ! Nominal COP {W/W}, REQ, #3
  autosize,             ! Design Chilled Water Flow Rate {m3/s}, AS, #4
  autosize,             ! Design Condenser Water Flow Rate {m3/s}, AS, #5
  Chiller 2 CHW Inlet,  ! Chilled Water Inlet Node Name REQ, #6
  Chiller 2 CHW Outlet, ! Chilled Water Outlet Node Name REQ, #7
  Chiller 2 CW Inlet,   ! Condenser Inlet Node Name #8
  Chiller 2 CW Outlet,  ! Condenser Outlet Node Name #9
  AirCooled,            ! Condenser Type Def: AirCooled, [AirCooled, WaterCooled, EvaporativelyCooled], #10
  NotModulated,         ! Chiller Flow Mode Def: NotModulated, [ConstantFlow, LeavingSetpointModulated, NotModulated], #11
  1.0;                  ! Sizing Factor Def: 1.0, #12

Chiller:ConstantCOP,
  Chiller 3,            ! Name RefList: [Chillers, validPlantEquipmentNames, validBranchEquipmentNames], RefClassList: [validPlantEquipmentTypes, validBranchEquipmentTypes], REQ, #1
  3516852.8,            ! Nominal Capacity {W}, AS, REQ, #2
  6,                    ! Nominal COP {W/W}, REQ, #3
  autosize,             ! Design Chilled Water Flow Rate {m3/s}, AS, #4
  autosize,             ! Design Condenser Water Flow Rate {m3/s}, AS, #5
  Chiller 3 CHW Inlet,  ! Chilled Water Inlet Node Name REQ, #6
  Chiller 3 CHW Outlet, ! Chilled Water Outlet Node Name REQ, #7
  Chiller 3 CW Inlet,   ! Condenser Inlet Node Name #8
  Chiller 3 CW Outlet,  ! Condenser Outlet Node Name #9
  AirCooled,            ! Condenser Type Def: AirCooled, [AirCooled, WaterCooled, EvaporativelyCooled], #10
  NotModulated,         ! Chiller Flow Mode Def: NotModulated, [ConstantFlow, LeavingSetpointModulated, NotModulated], #11
  1.0;                  ! Sizing Factor Def: 1.0, #12

```

Neat, right? So perhaps the simplest way that `bemp` can help you with
your current files is by breaking them up into smaller pieces.

To note here:

- The string is a relative file path to the file to import.
- All variables are carried along through the import.
