# mono --profile=log:report,noalloc GameWorld2_TextView.exe

LD_LIBRARY_PATH=/Library/Frameworks/Mono.framework/Versions/Current/lib  mono --profile=log:calls GameWorld2_TextView.exe

Overwrite outfile and limit stack depth:
LD_LIBRARY_PATH=/Library/Frameworks/Mono.framework/Versions/Current/lib  mono --profile=log:calls,calldepth=10,output=out.mlpd GameWorld2_TextView.exe

No alloc:
LD_LIBRARY_PATH=/Library/Frameworks/Mono.framework/Versions/Current/lib  mono --profile=log:calls,calldepth=10,output=out.mlpd,noalloc,report GameWorld2_TextView.exe

Alloc:
LD_LIBRARY_PATH=/Library/Frameworks/Mono.framework/Versions/Current/lib  mono --profile=log:calls,alloc,calldepth=10,output=out.mlpd,report GameWorld2_TextView.exe

mono --gc=sgen --profile=log:heapshot MyProgram.exe