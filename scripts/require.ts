// tsc of course has no clue that calls to require are resolved by browserify. We therefore pretend that the function
// exists.  
declare function require(path: string): any;
