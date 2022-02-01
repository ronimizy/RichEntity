# RE1000: Invalid String Literal Member Name Declaration

## Cause
Entity member with name that was specified in string literal does not exist.

## Rule description
When using `EntityFrameworkCore` `ModelBuilder` and `EntityTypebuilder` to configure hidden members string literals are used. Existance validation of member 
called by provided string literal name is run-time in `EntityFrameworkCore`, this analyzer performs this check at pre compile-time.

## How to fix violations
If your code in non-compliant with `RE1000` there is mismatch between your model configuration and entity. Check if member with name that is provided by
string literal exists in your entity types.

## When to suppress warnings
If you are declaring a shadow property on your entity model it is fair to suppress `RE1000`.

## Example of a violation
https://github.com/ronimizy/RichEntity/blob/88caa83e48ba9e99686f5cd05095693cf5c11a45/Analyzers/RichEntity.Analyzers.Sample/Invalid.cs#L11-L62


### Description
The model configuration in the example has `RE1000` diagnostic reported at every string literal. Provided code snippet covers two ways model 
configuration `ModelBuilder` and `EntityTypebuilder` (both with mistakes). 
- `"_childrens"` : The literal has a typo an additional `s` at the end.
- `"name"` / `"father"` : The literals are missing the `_` character at the beginning.
- `"_friends"` : The literal refers to member that was deleted or that is not yet created.

## Example of how to fix
https://github.com/ronimizy/RichEntity/blob/88caa83e48ba9e99686f5cd05095693cf5c11a45/Analyzers/RichEntity.Analyzers.Sample/Proper.cs#L11-L62

### Description
The typo was fixed, `_` characters were added, and `_friends` member was implemented.
