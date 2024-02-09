
/*
1. YES
2. YES
3. YES
4. NO
5. YES
6. YES
7. YES
8. NO
9. NO
10. YES
*/

?-op(140, fy, neg).
?-op(160, xfy, [and, or, imp, revimp, uparrow, downarrow, notimp, notrevimp, equiv, notequiv]).

/* member(Item, List) :- Item occurs in List. */

member(X, [X | _]).

member(X, [_ | Tail]) :-
    member(X, Tail).

/* remove(Item, List, Newlist) :- Newlist is the result of removing all occurences of Item from List. */

remove(X, [], []).

remove(X, [X | Tail], Newtail) :-
    remove(X, Tail, Newtail).

remove(X, [Head | Tail], [Head | Newtail]) :-
    remove(X, Tail, Newtail).

/* conjuctive(X) :- X is an alpha formula. */

conjunctive(_ and _).
conjunctive(neg(_ or _)).
conjunctive(neg(_ imp _)).
conjunctive(neg(_ revimp _)).
conjunctive(neg(_ uparrow _)).
conjunctive(_ downarrow _).
conjunctive(_ notimp _).
conjunctive(_ notrevimp _).


conjunctive(_ equiv _).
conjunctive(_ notequiv _).
conjunctive(neg(_ equiv _)).
conjunctive(neg(_ notequiv _)).

/* disjunctive(X) :- X is a beta formula. */

disjunctive(neg(_ and _)).
disjunctive(_ or _).
disjunctive(_ imp _).
disjunctive(_ revimp _).
disjunctive(_ uparrow _).
disjunctive(neg(_ downarrow _)).
disjunctive(neg(_ notimp _)).
disjunctive(neg(_ notrevimp _)).

/* unary(X) :- X is a double negation, or a negated constant. */

unary(neg neg _).
unary(neg true).
unary(neg false).

/* components(X, Y, Z) :- Y and Z are the components of the formula X, as defined in the alpha and beta tables. */

components(X and Y, X, Y).
components(neg(X and Y), neg X, neg Y).
components(X or Y, X, Y).
components(neg(X or Y), neg X, neg Y).
components(X imp Y, neg X, Y).
components(neg(X imp Y), X, neg Y).
components(X revimp Y, X, neg Y).
components(neg(X revimp Y), neg X, Y).
components(X uparrow Y, neg X, neg Y).
components(neg(X uparrow Y), X, Y).
components(X downarrow Y, neg X, neg Y).
components(neg(X downarrow Y), X, Y).
components(X notimp Y, X, neg Y).
components(neg(X notimp Y), neg X, Y).
components(X notrevimp Y, neg X, Y).
components(neg(X notrevimp Y), X, neg Y).

components(X equiv Y, X or neg Y, neg X or Y).
components(X notequiv Y,X or Y, neg X or neg Y).
components(neg(X equiv Y), X or Y, neg X or neg Y).
components(neg(X notequiv Y), X or neg Y, neg X or Y).


/* component(X, Y) :- Y is the component of the unary formula X. */

component(neg neg X, X).
component(neg true, false).
component(neg false, true).

/*Gets the negation of a statement*/
negcomponent(neg X, X).
negcomponent(X, neg X).

/* singlestep(Old, New) :- New is the result of applying a single step of the expansion process to Old, which is a generalized disjunction of generalized conjunctions. */

singlestep([Disjunction | Rest], New) :-
    member(Formula, Disjunction),
    unary(Formula),
    component(Formula, Newformula),
    remove(Formula, Disjunction, Temporary),
    Newdisjunction = [Newformula | Temporary],
    New = [Newdisjunction | Rest].

singlestep([Disjunction | Rest], New) :-
    member(Alpha, Disjunction),
    conjunctive(Alpha) ,
    components(Alpha, Alphaone, Alphatwo),
    remove(Alpha, Disjunction, Temporary),
	  Newdisone = [Alphaone | Temporary],
    Newdistwo = [Alphatwo | Temporary],
    New = [Newdisone, Newdistwo | Rest].

singlestep([Disjunction | Rest], New) :-
    member(Beta, Disjunction),
    disjunctive(Beta),
    components(Beta, Betaone, Betatwo),
    remove(Beta, Disjunction, Temporary),
    Newdis = [Betaone, Betatwo | Temporary],
    New = [Newdis | Rest].

singlestep([Disjunction | Rest], [Disjunction | Newrest]) :-
    singlestep(Rest, Newrest).

/* expand(Old, New) :- New is the result of applying singlestep as many times as possible, starting with Old. */

expand(Dis, Newdis) :-
    singlestep(Dis, Temp),
    expand(Temp, Newdis).

expand(Dis, Dis).

/* dualclauseform(X, Y) :- Y is the dual clause form of X. */

clauseform(X, Y) :- expand([[X]], Y).

/* clean(Old, New) :- remove duplicates and negations in disjunctions. */

clean([],[]).

clean([Disjunction | Rest], New) :-
    findNegations(Disjunction,Disjunction),
    clean(Rest,New).


clean([Disjunction | Rest], [Newhead | Newtail]) :-
    removeDupes(Disjunction,Newhead),
    clean(Rest,Newtail).

/* findNegations(Old,New) :- Is true only if a value and its negation do not exist in a list.*/

findNegations([Statement | Rest], Disjunction) :-
    negcomponent(Statement,Negation),
    member(Negation, Disjunction).

findNegations([Statement | Rest], Disjunction) :-
    findNegations(Rest,Disjunction).

/* removeDupes(Old,New) :- makes it so a list can only have one of each value.*/

removeDupes([],[]).

removeDupes([Statement | Rest], New) :-
    remove(Statement, Rest, Temp),
    removeDupes(Temp,Temp2),
    New = [Statement|Temp2].

/* resolutionstep(Old,New) :- Perform a step of resolution.*/

resolutionstep(Resolution,New) :-
    resolve1(Resolution,Resolution,New).

resolve1([Disjunction | Rest], All, New) :-
    resolve2(Disjunction, Disjunction, All, Rest, Temp),
    New = [Temp,Disjunction|Rest].

resolve1([Disjunction | Rest], All, [Disjunction|Newtail]) :-
    resolve1(Rest,All,Newtail).

/*Iterate theough each statement in the current disjunction*/

resolve2([Statement|Rest], CurDis, All, Disjunctions, New) :-
    resolve3(Statement,CurDis,Disjunctions,All,New).

resolve2([Statement|Rest], CurDis, All, Disjunctions, New) :-
    resolve2(Rest,CurDis,All, Disjunctions, New).

/*Find negations of statements in other disjunctions*/

resolve3(Statement,CurDis,[Disjunction|Rest],All,New) :-
    resolve3(Statement,CurDis,Rest,All,New).

resolve3(Statement,CurDis,[Disjunction|Rest],All,New) :-
    negcomponent(Statement,Negation),
    member(Negation,Disjunction),
    append(CurDis,Disjunction,NewDis1),
    remove(Statement,NewDis1,NewDis2),
    remove(Negation,NewDis2,NewDis3),
    removeDupes(NewDis3,New),
    !,
    not(member(New, All)),
    !,
    not(findNegations(New,New)).

/* resolution(Old,New) :- Perform as many resolution steps as possible */

resolution(Old, New) :-
    resolutionstep(Old, Temp),
    resolution(Temp, New).

resolution(Old,Old).

/* closed(Resolution :- A disjunction is false. */


closed(Resolution) :-
    member([], Resolution).



/* test(X) :- create a complete resolution expansion for neg X, and see if it is closed.*/

test(X) :-
    clauseform(neg X, Temp),
    clean(Temp, Temp2),
    if_then_else(resolve_and_close(Temp2), yes, no).

yes :-
    write("YES").

no :-
    write("NO").

/* if_then_else(P, Q, R) :- either P and Q, or not P and not R. */

if_then_else(P, Q, R) :-
    P,
    !,
    Q.

if_then_else(P, Q, R) :-
    R.

/* expand_and_close(Resolution) :- some expansion of a Resolution. */

resolve_and_close(Resolution) :-
    closed(Resolution).

resolve_and_close(Resolution) :-
    resolutionstep(Resolution,Temp),
    resolve_and_close(Temp).
