# ABCSharp
### A Derivative Work
Currently, this is a 'blind idiot port' of [pyabc](https://github.com/campagnola/pyabc).
It isn't perfect, and not everything in the original repo was ported.

A lot of code isn't clean because C# isn't very pythonic, and many pieces of code were transcribed as they were.


#### Future Plans
While I like pyabc, as of my writing it is in 'Pre-Alpha' and is something of a hybrid lexer/parser, I'd like to push it the extra mile and make ABCSharp an actual lexer/parser instead of the hybrid it currently is.

I'll likely keep a 'pyabc port' branch for posterity's sake.

### Immediate Problems
But; it seems ABC's grammar can be confusing; 
% is a [comment](http://abcnotation.com/wiki/abc:standard:v2.1#comments_and_remarks)  
%% is a [stylesheet derective](http://abcnotation.com/wiki/abc:standard:v2.1#stylesheet_directives)
And while the [official documentation](http://abcnotation.com/wiki/abc:standard:v2.1) is very verbose, it no longer provides obvious grammar diagrams (the two BNF diagrams listed are no longer available), making it harder to write a full parser without extensive knowledge of the format.

But perhaps the worst problem, is that the format has a few intended uses; and a catch-all parser is likely too much for me to handle.

#### Legal
I'm not a legal expert, but the original is licensed under  the MIT License; and the source can be found [here](https://github.com/campagnola/pyabc).
As required, a copy of their license has been added to the repo.