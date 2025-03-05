## Adding a Built-in Function

Currently a bit of a mess.

1. Add subclass of `FunctionExpression`.
2. Typical constructor will be empty and look like:

   ```C#
   public FoldFunctionExpression() : base(new List<Dictionary<string, Expression>>(), new List<string>{ "list", "func", "initial" }, FileType.Any)
   ```
3. Override `Evaluate` function
   1. Check count and types of arguments.
   2. Evaluate the function as desired.

4. Add the functions to the environment dictionaries in the constructors for `IdfPlusExpVisitor` and `IdfPlusVisitor`.
5. Add appropriate documentation.
