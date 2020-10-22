grammar Idfplus;

options {
    language=CSharp;
}

variable_declaration : 
    VARIABLE '=' expression;

expression : STRING | NUMERIC | VARIABLE | list | data_statement | map_statement ;

list : '[' expression* ']' ;

template_statement : TEMPLATE_KEYWORD FUNCTION_NAME VARIABLE* STRING ;

map_statement : MAP_KEYWORD FUNCTION_NAME (list | VARIABLE) ;

import_statement : 'import' STRING ;

print_statment : 'print' expression ;

data_statement : 'data' STRING ;

idf : (COMMENT | object | variable_declaration | template_statement | import_statement | print_statment)* EOF;

/*object : ALPHA FIELD_SEPARATOR COMMENT* fields ;*/

object : OBJECT ;

OBJECT : OBJECT_TYPE .*? OBJECT_TERMINATOR COMMENT* ;

MAP_KEYWORD : 'map' ;
TEMPLATE_KEYWORD : 'template' ;

FUNCTION_NAME : [a-zA-Z][a-zA-Z0-9]* ;

COMMENT :  '!' .*? '\n' ;

OBJECT_TERMINATOR : ';' ;

NUMERIC : '-'?(([1-9][0-9]*|'0')('.'[0-9]+)? | ('.'[0-9]+))([eE]'-'?[0-9]+)? ;

STRING : '\'' .*? '\'' ;

VARIABLE : '$' [a-zA-Z0-9]+ ('.' [a-zA-Z0-9]+)* ;

WS : [ \t\r\n]+ -> skip ;

fragment A:('a'|'A');
fragment B:('b'|'B');
fragment C:('c'|'C');
fragment D:('d'|'D');
fragment E:('e'|'E');
fragment F:('f'|'F');
fragment G:('g'|'G');
fragment H:('h'|'H');
fragment I:('i'|'I');
fragment J:('j'|'J');
fragment K:('k'|'K');
fragment L:('l'|'L');
fragment M:('m'|'M');
fragment N:('n'|'N');
fragment O:('o'|'O');
fragment P:('p'|'P');
fragment Q:('q'|'Q');
fragment R:('r'|'R');
fragment S:('s'|'S');
fragment T:('t'|'T');
fragment U:('u'|'U');
fragment V:('v'|'V');
fragment W:('w'|'W');
fragment X:('x'|'X');
fragment Y:('y'|'Y');
fragment Z:('z'|'Z');

fragment OBJECT_TYPE : 
  V E R S I O N |
  S I M U L A T I O N C O N T R O L |
  P E R F O R M A N C E P R E C I S I O N T R A D E O F F S |
  B U I L D I N G |
  S H A D O W C A L C U L A T I O N |
  S U R F A C E C O N V E C T I O N A L G O R I T H M ':' I N S I D E |
  S U R F A C E C O N V E C T I O N A L G O R I T H M ':' O U T S I D E |
  H E A T B A L A N C E A L G O R I T H M |
  H E A T B A L A N C E S E T T I N G S ':' C O N D U C T I O N F I N I T E D I F F E R E N C E |
  Z O N E A I R H E A T B A L A N C E A L G O R I T H M |
  Z O N E A I R C O N T A M I N A N T B A L A N C E |
  Z O N E A I R M A S S F L O W C O N S E R V A T I O N |
  Z O N E C A P A C I T A N C E M U L T I P L I E R ':' R E S E A R C H S P E C I A L |
  T I M E S T E P |
  C O N V E R G E N C E L I M I T S |
  H V A C S Y S T E M R O O T F I N D I N G A L G O R I T H M |
  C O M P L I A N C E ':' B U I L D I N G |
  S I T E ':' L O C A T I O N |
  S I T E ':' V A R I A B L E L O C A T I O N |
  S I Z I N G P E R I O D ':' D E S I G N D A Y |
  S I Z I N G P E R I O D ':' W E A T H E R F I L E D A Y S |
  S I Z I N G P E R I O D ':' W E A T H E R F I L E C O N D I T I O N T Y P E |
  R U N P E R I O D |
  R U N P E R I O D C O N T R O L ':' S P E C I A L D A Y S |
  R U N P E R I O D C O N T R O L ':' D A Y L I G H T S A V I N G T I M E |
  W E A T H E R P R O P E R T Y ':' S K Y T E M P E R A T U R E |
  S I T E ':' W E A T H E R S T A T I O N |
  S I T E ':' H E I G H T V A R I A T I O N |
  S I T E ':' G R O U N D T E M P E R A T U R E ':' B U I L D I N G S U R F A C E |
  S I T E ':' G R O U N D T E M P E R A T U R E ':' F C F A C T O R M E T H O D |
  S I T E ':' G R O U N D T E M P E R A T U R E ':' S H A L L O W |
  S I T E ':' G R O U N D T E M P E R A T U R E ':' D E E P |
  S I T E ':' G R O U N D T E M P E R A T U R E ':' U N D I S T U R B E D ':' F I N I T E D I F F E R E N C E |
  S I T E ':' G R O U N D T E M P E R A T U R E ':' U N D I S T U R B E D ':' K U S U D A A C H E N B A C H |
  S I T E ':' G R O U N D T E M P E R A T U R E ':' U N D I S T U R B E D ':' X I N G |
  S I T E ':' G R O U N D D O M A I N ':' S L A B |
  S I T E ':' G R O U N D D O M A I N ':' B A S E M E N T |
  S I T E ':' G R O U N D R E F L E C T A N C E |
  S I T E ':' G R O U N D R E F L E C T A N C E ':' S N O W M O D I F I E R |
  S I T E ':' W A T E R M A I N S T E M P E R A T U R E |
  S I T E ':' P R E C I P I T A T I O N |
  R O O F I R R I G A T I O N |
  S I T E ':' S O L A R A N D V I S I B L E S P E C T R U M |
  S I T E ':' S P E C T R U M D A T A |
  S C H E D U L E T Y P E L I M I T S |
  S C H E D U L E ':' D A Y ':' H O U R L Y |
  S C H E D U L E ':' D A Y ':' I N T E R V A L |
  S C H E D U L E ':' D A Y ':' L I S T |
  S C H E D U L E ':' W E E K ':' D A I L Y |
  S C H E D U L E ':' W E E K ':' C O M P A C T |
  S C H E D U L E ':' Y E A R |
  S C H E D U L E ':' C O M P A C T |
  S C H E D U L E ':' C O N S T A N T |
  S C H E D U L E ':' F I L E ':' S H A D I N G |
  S C H E D U L E ':' F I L E |
  M A T E R I A L |
  M A T E R I A L ':' N O M A S S |
  M A T E R I A L ':' I N F R A R E D T R A N S P A R E N T |
  M A T E R I A L ':' A I R G A P |
  M A T E R I A L ':' R O O F V E G E T A T I O N |
  W I N D O W M A T E R I A L ':' S I M P L E G L A Z I N G S Y S T E M |
  W I N D O W M A T E R I A L ':' G L A Z I N G |
  W I N D O W M A T E R I A L ':' G L A Z I N G G R O U P ':' T H E R M O C H R O M I C |
  W I N D O W M A T E R I A L ':' G L A Z I N G ':' R E F R A C T I O N E X T I N C T I O N M E T H O D |
  W I N D O W M A T E R I A L ':' G A S |
  W I N D O W G A P ':' S U P P O R T P I L L A R |
  W I N D O W G A P ':' D E F L E C T I O N S T A T E |
  W I N D O W M A T E R I A L ':' G A S M I X T U R E |
  W I N D O W M A T E R I A L ':' G A P |
  W I N D O W M A T E R I A L ':' S H A D E |
  W I N D O W M A T E R I A L ':' C O M P L E X S H A D E |
  W I N D O W M A T E R I A L ':' B L I N D |
  W I N D O W M A T E R I A L ':' S C R E E N |
  W I N D O W M A T E R I A L ':' S H A D E ':' E Q U I V A L E N T L A Y E R |
  W I N D O W M A T E R I A L ':' D R A P E ':' E Q U I V A L E N T L A Y E R |
  W I N D O W M A T E R I A L ':' B L I N D ':' E Q U I V A L E N T L A Y E R |
  W I N D O W M A T E R I A L ':' S C R E E N ':' E Q U I V A L E N T L A Y E R |
  W I N D O W M A T E R I A L ':' G L A Z I N G ':' E Q U I V A L E N T L A Y E R |
  W I N D O W M A T E R I A L ':' G A P ':' E Q U I V A L E N T L A Y E R |
  M A T E R I A L P R O P E R T Y ':' M O I S T U R E P E N E T R A T I O N D E P T H ':' S E T T I N G S |
  M A T E R I A L P R O P E R T Y ':' P H A S E C H A N G E |
  M A T E R I A L P R O P E R T Y ':' P H A S E C H A N G E H Y S T E R E S I S |
  M A T E R I A L P R O P E R T Y ':' V A R I A B L E T H E R M A L C O N D U C T I V I T Y |
  M A T E R I A L P R O P E R T Y ':' H E A T A N D M O I S T U R E T R A N S F E R ':' S E T T I N G S |
  M A T E R I A L P R O P E R T Y ':' H E A T A N D M O I S T U R E T R A N S F E R ':' S O R P T I O N I S O T H E R M |
  M A T E R I A L P R O P E R T Y ':' H E A T A N D M O I S T U R E T R A N S F E R ':' S U C T I O N |
  M A T E R I A L P R O P E R T Y ':' H E A T A N D M O I S T U R E T R A N S F E R ':' R E D I S T R I B U T I O N |
  M A T E R I A L P R O P E R T Y ':' H E A T A N D M O I S T U R E T R A N S F E R ':' D I F F U S I O N |
  M A T E R I A L P R O P E R T Y ':' H E A T A N D M O I S T U R E T R A N S F E R ':' T H E R M A L C O N D U C T I V I T Y |
  M A T E R I A L P R O P E R T Y ':' G L A Z I N G S P E C T R A L D A T A |
  C O N S T R U C T I O N |
  C O N S T R U C T I O N ':' C F A C T O R U N D E R G R O U N D W A L L |
  C O N S T R U C T I O N ':' F F A C T O R G R O U N D F L O O R |
  C O N S T R U C T I O N ':' I N T E R N A L S O U R C E |
  C O N S T R U C T I O N ':' A I R B O U N D A R Y |
  W I N D O W T H E R M A L M O D E L ':' P A R A M S |
  W I N D O W S C A L C U L A T I O N E N G I N E |
  C O N S T R U C T I O N ':' C O M P L E X F E N E S T R A T I O N S T A T E |
  C O N S T R U C T I O N ':' W I N D O W E Q U I V A L E N T L A Y E R |
  C O N S T R U C T I O N ':' W I N D O W D A T A F I L E |
  G L O B A L G E O M E T R Y R U L E S |
  G E O M E T R Y T R A N S F O R M |
  Z O N E |
  Z O N E L I S T |
  Z O N E G R O U P |
  B U I L D I N G S U R F A C E ':' D E T A I L E D |
  W A L L ':' D E T A I L E D |
  R O O F C E I L I N G ':' D E T A I L E D |
  F L O O R ':' D E T A I L E D |
  W A L L ':' E X T E R I O R |
  W A L L ':' A D I A B A T I C |
  W A L L ':' U N D E R G R O U N D |
  W A L L ':' I N T E R Z O N E |
  R O O F |
  C E I L I N G ':' A D I A B A T I C |
  C E I L I N G ':' I N T E R Z O N E |
  F L O O R ':' G R O U N D C O N T A C T |
  F L O O R ':' A D I A B A T I C |
  F L O O R ':' I N T E R Z O N E |
  F E N E S T R A T I O N S U R F A C E ':' D E T A I L E D |
  W I N D O W |
  D O O R |
  G L A Z E D D O O R |
  W I N D O W ':' I N T E R Z O N E |
  D O O R ':' I N T E R Z O N E |
  G L A Z E D D O O R ':' I N T E R Z O N E |
  W I N D O W S H A D I N G C O N T R O L |
  W I N D O W P R O P E R T Y ':' F R A M E A N D D I V I D E R |
  W I N D O W P R O P E R T Y ':' A I R F L O W C O N T R O L |
  W I N D O W P R O P E R T Y ':' S T O R M W I N D O W |
  I N T E R N A L M A S S |
  S H A D I N G ':' S I T E |
  S H A D I N G ':' B U I L D I N G |
  S H A D I N G ':' S I T E ':' D E T A I L E D |
  S H A D I N G ':' B U I L D I N G ':' D E T A I L E D |
  S H A D I N G ':' O V E R H A N G |
  S H A D I N G ':' O V E R H A N G ':' P R O J E C T I O N |
  S H A D I N G ':' F I N |
  S H A D I N G ':' F I N ':' P R O J E C T I O N |
  S H A D I N G ':' Z O N E ':' D E T A I L E D |
  S H A D I N G P R O P E R T Y ':' R E F L E C T A N C E |
  S U R F A C E P R O P E R T Y ':' H E A T T R A N S F E R A L G O R I T H M |
  S U R F A C E P R O P E R T Y ':' H E A T T R A N S F E R A L G O R I T H M ':' M U L T I P L E S U R F A C E |
  S U R F A C E P R O P E R T Y ':' H E A T T R A N S F E R A L G O R I T H M ':' S U R F A C E L I S T |
  S U R F A C E P R O P E R T Y ':' H E A T T R A N S F E R A L G O R I T H M ':' C O N S T R U C T I O N |
  S U R F A C E P R O P E R T Y ':' H E A T B A L A N C E S O U R C E T E R M |
  S U R F A C E C O N T R O L ':' M O V A B L E I N S U L A T I O N |
  S U R F A C E P R O P E R T Y ':' O T H E R S I D E C O E F F I C I E N T S |
  S U R F A C E P R O P E R T Y ':' O T H E R S I D E C O N D I T I O N S M O D E L |
  S U R F A C E P R O P E R T Y ':' U N D E R W A T E R |
  F O U N D A T I O N ':' K I V A |
  F O U N D A T I O N ':' K I V A ':' S E T T I N G S |
  S U R F A C E P R O P E R T Y ':' E X P O S E D F O U N D A T I O N P E R I M E T E R |
  S U R F A C E C O N V E C T I O N A L G O R I T H M ':' I N S I D E ':' A D A P T I V E M O D E L S E L E C T I O N S |
  S U R F A C E C O N V E C T I O N A L G O R I T H M ':' O U T S I D E ':' A D A P T I V E M O D E L S E L E C T I O N S |
  S U R F A C E C O N V E C T I O N A L G O R I T H M ':' I N S I D E ':' U S E R C U R V E |
  S U R F A C E C O N V E C T I O N A L G O R I T H M ':' O U T S I D E ':' U S E R C U R V E |
  S U R F A C E P R O P E R T Y ':' C O N V E C T I O N C O E F F I C I E N T S |
  S U R F A C E P R O P E R T Y ':' C O N V E C T I O N C O E F F I C I E N T S ':' M U L T I P L E S U R F A C E |
  S U R F A C E P R O P E R T I E S ':' V A P O R C O E F F I C I E N T S |
  S U R F A C E P R O P E R T Y ':' E X T E R I O R N A T U R A L V E N T E D C A V I T Y |
  S U R F A C E P R O P E R T Y ':' S O L A R I N C I D E N T I N S I D E |
  S U R F A C E P R O P E R T Y ':' L O C A L E N V I R O N M E N T |
  Z O N E P R O P E R T Y ':' L O C A L E N V I R O N M E N T |
  S U R F A C E P R O P E R T Y ':' S U R R O U N D I N G S U R F A C E S |
  C O M P L E X F E N E S T R A T I O N P R O P E R T Y ':' S O L A R A B S O R B E D L A Y E R S |
  Z O N E P R O P E R T Y ':' U S E R V I E W F A C T O R S ':' B Y S U R F A C E N A M E |
  G R O U N D H E A T T R A N S F E R ':' C O N T R O L |
  G R O U N D H E A T T R A N S F E R ':' S L A B ':' M A T E R I A L S |
  G R O U N D H E A T T R A N S F E R ':' S L A B ':' M A T L P R O P S |
  G R O U N D H E A T T R A N S F E R ':' S L A B ':' B O U N D C O N D S |
  G R O U N D H E A T T R A N S F E R ':' S L A B ':' B L D G P R O P S |
  G R O U N D H E A T T R A N S F E R ':' S L A B ':' I N S U L A T I O N |
  G R O U N D H E A T T R A N S F E R ':' S L A B ':' E Q U I V A L E N T S L A B |
  G R O U N D H E A T T R A N S F E R ':' S L A B ':' A U T O G R I D |
  G R O U N D H E A T T R A N S F E R ':' S L A B ':' M A N U A L G R I D |
  G R O U N D H E A T T R A N S F E R ':' S L A B ':' X F A C E |
  G R O U N D H E A T T R A N S F E R ':' S L A B ':' Y F A C E |
  G R O U N D H E A T T R A N S F E R ':' S L A B ':' Z F A C E |
  G R O U N D H E A T T R A N S F E R ':' B A S E M E N T ':' S I M P A R A M E T E R S |
  G R O U N D H E A T T R A N S F E R ':' B A S E M E N T ':' M A T L P R O P S |
  G R O U N D H E A T T R A N S F E R ':' B A S E M E N T ':' I N S U L A T I O N |
  G R O U N D H E A T T R A N S F E R ':' B A S E M E N T ':' S U R F A C E P R O P S |
  G R O U N D H E A T T R A N S F E R ':' B A S E M E N T ':' B L D G D A T A |
  G R O U N D H E A T T R A N S F E R ':' B A S E M E N T ':' I N T E R I O R |
  G R O U N D H E A T T R A N S F E R ':' B A S E M E N T ':' C O M B L D G |
  G R O U N D H E A T T R A N S F E R ':' B A S E M E N T ':' E Q U I V S L A B |
  G R O U N D H E A T T R A N S F E R ':' B A S E M E N T ':' E Q U I V A U T O G R I D |
  G R O U N D H E A T T R A N S F E R ':' B A S E M E N T ':' A U T O G R I D |
  G R O U N D H E A T T R A N S F E R ':' B A S E M E N T ':' M A N U A L G R I D |
  G R O U N D H E A T T R A N S F E R ':' B A S E M E N T ':' X F A C E |
  G R O U N D H E A T T R A N S F E R ':' B A S E M E N T ':' Y F A C E |
  G R O U N D H E A T T R A N S F E R ':' B A S E M E N T ':' Z F A C E |
  R O O M A I R M O D E L T Y P E |
  R O O M A I R ':' T E M P E R A T U R E P A T T E R N ':' U S E R D E F I N E D |
  R O O M A I R ':' T E M P E R A T U R E P A T T E R N ':' C O N S T A N T G R A D I E N T |
  R O O M A I R ':' T E M P E R A T U R E P A T T E R N ':' T W O G R A D I E N T |
  R O O M A I R ':' T E M P E R A T U R E P A T T E R N ':' N O N D I M E N S I O N A L H E I G H T |
  R O O M A I R ':' T E M P E R A T U R E P A T T E R N ':' S U R F A C E M A P P I N G |
  R O O M A I R ':' N O D E |
  R O O M A I R S E T T I N G S ':' O N E N O D E D I S P L A C E M E N T V E N T I L A T I O N |
  R O O M A I R S E T T I N G S ':' T H R E E N O D E D I S P L A C E M E N T V E N T I L A T I O N |
  R O O M A I R S E T T I N G S ':' C R O S S V E N T I L A T I O N |
  R O O M A I R S E T T I N G S ':' U N D E R F L O O R A I R D I S T R I B U T I O N I N T E R I O R |
  R O O M A I R S E T T I N G S ':' U N D E R F L O O R A I R D I S T R I B U T I O N E X T E R I O R |
  R O O M A I R ':' N O D E ':' A I R F L O W N E T W O R K |
  R O O M A I R ':' N O D E ':' A I R F L O W N E T W O R K ':' A D J A C E N T S U R F A C E L I S T |
  R O O M A I R ':' N O D E ':' A I R F L O W N E T W O R K ':' I N T E R N A L G A I N S |
  R O O M A I R ':' N O D E ':' A I R F L O W N E T W O R K ':' H V A C E Q U I P M E N T |
  R O O M A I R S E T T I N G S ':' A I R F L O W N E T W O R K |
  P E O P L E |
  C O M F O R T V I E W F A C T O R A N G L E S |
  L I G H T S |
  E L E C T R I C E Q U I P M E N T |
  G A S E Q U I P M E N T |
  H O T W A T E R E Q U I P M E N T |
  S T E A M E Q U I P M E N T |
  O T H E R E Q U I P M E N T |
  E L E C T R I C E Q U I P M E N T ':' I T E ':' A I R C O O L E D |
  Z O N E B A S E B O A R D ':' O U T D O O R T E M P E R A T U R E C O N T R O L L E D |
  S W I M M I N G P O O L ':' I N D O O R |
  Z O N E C O N T A M I N A N T S O U R C E A N D S I N K ':' C A R B O N D I O X I D E |
  Z O N E C O N T A M I N A N T S O U R C E A N D S I N K ':' G E N E R I C ':' C O N S T A N T |
  S U R F A C E C O N T A M I N A N T S O U R C E A N D S I N K ':' G E N E R I C ':' P R E S S U R E D R I V E N |
  Z O N E C O N T A M I N A N T S O U R C E A N D S I N K ':' G E N E R I C ':' C U T O F F M O D E L |
  Z O N E C O N T A M I N A N T S O U R C E A N D S I N K ':' G E N E R I C ':' D E C A Y S O U R C E |
  S U R F A C E C O N T A M I N A N T S O U R C E A N D S I N K ':' G E N E R I C ':' B O U N D A R Y L A Y E R D I F F U S I O N |
  S U R F A C E C O N T A M I N A N T S O U R C E A N D S I N K ':' G E N E R I C ':' D E P O S I T I O N V E L O C I T Y S I N K |
  Z O N E C O N T A M I N A N T S O U R C E A N D S I N K ':' G E N E R I C ':' D E P O S I T I O N R A T E S I N K |
  D A Y L I G H T I N G ':' C O N T R O L S |
  D A Y L I G H T I N G ':' R E F E R E N C E P O I N T |
  D A Y L I G H T I N G ':' D E L I G H T ':' C O M P L E X F E N E S T R A T I O N |
  D A Y L I G H T I N G D E V I C E ':' T U B U L A R |
  D A Y L I G H T I N G D E V I C E ':' S H E L F |
  D A Y L I G H T I N G D E V I C E ':' L I G H T W E L L |
  O U T P U T ':' D A Y L I G H T F A C T O R S |
  O U T P U T ':' I L L U M I N A N C E M A P |
  O U T P U T C O N T R O L ':' I L L U M I N A N C E M A P ':' S T Y L E |
  Z O N E I N F I L T R A T I O N ':' D E S I G N F L O W R A T E |
  Z O N E I N F I L T R A T I O N ':' E F F E C T I V E L E A K A G E A R E A |
  Z O N E I N F I L T R A T I O N ':' F L O W C O E F F I C I E N T |
  Z O N E V E N T I L A T I O N ':' D E S I G N F L O W R A T E |
  Z O N E V E N T I L A T I O N ':' W I N D A N D S T A C K O P E N A R E A |
  Z O N E A I R B A L A N C E ':' O U T D O O R A I R |
  Z O N E M I X I N G |
  Z O N E C R O S S M I X I N G |
  Z O N E R E F R I G E R A T I O N D O O R M I X I N G |
  Z O N E E A R T H T U B E |
  Z O N E C O O L T O W E R ':' S H O W E R |
  Z O N E T H E R M A L C H I M N E Y |
  A I R F L O W N E T W O R K ':' S I M U L A T I O N C O N T R O L |
  A I R F L O W N E T W O R K ':' M U L T I Z O N E ':' Z O N E |
  A I R F L O W N E T W O R K ':' M U L T I Z O N E ':' S U R F A C E |
  A I R F L O W N E T W O R K ':' M U L T I Z O N E ':' R E F E R E N C E C R A C K C O N D I T I O N S |
  A I R F L O W N E T W O R K ':' M U L T I Z O N E ':' S U R F A C E ':' C R A C K |
  A I R F L O W N E T W O R K ':' M U L T I Z O N E ':' S U R F A C E ':' E F F E C T I V E L E A K A G E A R E A |
  A I R F L O W N E T W O R K ':' M U L T I Z O N E ':' C O M P O N E N T ':' D E T A I L E D O P E N I N G |
  A I R F L O W N E T W O R K ':' M U L T I Z O N E ':' C O M P O N E N T ':' S I M P L E O P E N I N G |
  A I R F L O W N E T W O R K ':' M U L T I Z O N E ':' C O M P O N E N T ':' H O R I Z O N T A L O P E N I N G |
  A I R F L O W N E T W O R K ':' M U L T I Z O N E ':' C O M P O N E N T ':' Z O N E E X H A U S T F A N |
  A I R F L O W N E T W O R K ':' M U L T I Z O N E ':' E X T E R N A L N O D E |
  A I R F L O W N E T W O R K ':' M U L T I Z O N E ':' W I N D P R E S S U R E C O E F F I C I E N T A R R A Y |
  A I R F L O W N E T W O R K ':' M U L T I Z O N E ':' W I N D P R E S S U R E C O E F F I C I E N T V A L U E S |
  A I R F L O W N E T W O R K ':' Z O N E C O N T R O L ':' P R E S S U R E C O N T R O L L E R |
  A I R F L O W N E T W O R K ':' D I S T R I B U T I O N ':' N O D E |
  A I R F L O W N E T W O R K ':' D I S T R I B U T I O N ':' C O M P O N E N T ':' L E A K |
  A I R F L O W N E T W O R K ':' D I S T R I B U T I O N ':' C O M P O N E N T ':' L E A K A G E R A T I O |
  A I R F L O W N E T W O R K ':' D I S T R I B U T I O N ':' C O M P O N E N T ':' D U C T |
  A I R F L O W N E T W O R K ':' D I S T R I B U T I O N ':' C O M P O N E N T ':' F A N |
  A I R F L O W N E T W O R K ':' D I S T R I B U T I O N ':' C O M P O N E N T ':' C O I L |
  A I R F L O W N E T W O R K ':' D I S T R I B U T I O N ':' C O M P O N E N T ':' H E A T E X C H A N G E R |
  A I R F L O W N E T W O R K ':' D I S T R I B U T I O N ':' C O M P O N E N T ':' T E R M I N A L U N I T |
  A I R F L O W N E T W O R K ':' D I S T R I B U T I O N ':' C O M P O N E N T ':' C O N S T A N T P R E S S U R E D R O P |
  A I R F L O W N E T W O R K ':' D I S T R I B U T I O N ':' C O M P O N E N T ':' O U T D O O R A I R F L O W |
  A I R F L O W N E T W O R K ':' D I S T R I B U T I O N ':' C O M P O N E N T ':' R E L I E F A I R F L O W |
  A I R F L O W N E T W O R K ':' D I S T R I B U T I O N ':' L I N K A G E |
  A I R F L O W N E T W O R K ':' D I S T R I B U T I O N ':' D U C T V I E W F A C T O R S |
  A I R F L O W N E T W O R K ':' O C C U P A N T V E N T I L A T I O N C O N T R O L |
  A I R F L O W N E T W O R K ':' I N T R A Z O N E ':' N O D E |
  A I R F L O W N E T W O R K ':' I N T R A Z O N E ':' L I N K A G E |
  E X T E R I O R ':' L I G H T S |
  E X T E R I O R ':' F U E L E Q U I P M E N T |
  E X T E R I O R ':' W A T E R E Q U I P M E N T |
  H V A C T E M P L A T E ':' T H E R M O S T A T |
  H V A C T E M P L A T E ':' Z O N E ':' I D E A L L O A D S A I R S Y S T E M |
  H V A C T E M P L A T E ':' Z O N E ':' B A S E B O A R D H E A T |
  H V A C T E M P L A T E ':' Z O N E ':' F A N C O I L |
  H V A C T E M P L A T E ':' Z O N E ':' P T A C |
  H V A C T E M P L A T E ':' Z O N E ':' P T H P |
  H V A C T E M P L A T E ':' Z O N E ':' W A T E R T O A I R H E A T P U M P |
  H V A C T E M P L A T E ':' Z O N E ':' V R F |
  H V A C T E M P L A T E ':' Z O N E ':' U N I T A R Y |
  H V A C T E M P L A T E ':' Z O N E ':' V A V |
  H V A C T E M P L A T E ':' Z O N E ':' V A V ':' F A N P O W E R E D |
  H V A C T E M P L A T E ':' Z O N E ':' V A V ':' H E A T A N D C O O L |
  H V A C T E M P L A T E ':' Z O N E ':' C O N S T A N T V O L U M E |
  H V A C T E M P L A T E ':' Z O N E ':' D U A L D U C T |
  H V A C T E M P L A T E ':' S Y S T E M ':' V R F |
  H V A C T E M P L A T E ':' S Y S T E M ':' U N I T A R Y |
  H V A C T E M P L A T E ':' S Y S T E M ':' U N I T A R Y H E A T P U M P ':' A I R T O A I R |
  H V A C T E M P L A T E ':' S Y S T E M ':' U N I T A R Y S Y S T E M |
  H V A C T E M P L A T E ':' S Y S T E M ':' V A V |
  H V A C T E M P L A T E ':' S Y S T E M ':' P A C K A G E D V A V |
  H V A C T E M P L A T E ':' S Y S T E M ':' C O N S T A N T V O L U M E |
  H V A C T E M P L A T E ':' S Y S T E M ':' D U A L D U C T |
  H V A C T E M P L A T E ':' S Y S T E M ':' D E D I C A T E D O U T D O O R A I R |
  H V A C T E M P L A T E ':' P L A N T ':' C H I L L E D W A T E R L O O P |
  H V A C T E M P L A T E ':' P L A N T ':' C H I L L E R |
  H V A C T E M P L A T E ':' P L A N T ':' C H I L L E R ':' O B J E C T R E F E R E N C E |
  H V A C T E M P L A T E ':' P L A N T ':' T O W E R |
  H V A C T E M P L A T E ':' P L A N T ':' T O W E R ':' O B J E C T R E F E R E N C E |
  H V A C T E M P L A T E ':' P L A N T ':' H O T W A T E R L O O P |
  H V A C T E M P L A T E ':' P L A N T ':' B O I L E R |
  H V A C T E M P L A T E ':' P L A N T ':' B O I L E R ':' O B J E C T R E F E R E N C E |
  H V A C T E M P L A T E ':' P L A N T ':' M I X E D W A T E R L O O P |
  D E S I G N S P E C I F I C A T I O N ':' O U T D O O R A I R |
  D E S I G N S P E C I F I C A T I O N ':' Z O N E A I R D I S T R I B U T I O N |
  S I Z I N G ':' P A R A M E T E R S |
  S I Z I N G ':' Z O N E |
  D E S I G N S P E C I F I C A T I O N ':' Z O N E H V A C ':' S I Z I N G |
  D E S I G N S P E C I F I C A T I O N ':' A I R T E R M I N A L ':' S I Z I N G |
  S I Z I N G ':' S Y S T E M |
  S I Z I N G ':' P L A N T |
  O U T P U T C O N T R O L ':' S I Z I N G ':' S T Y L E |
  Z O N E C O N T R O L ':' H U M I D I S T A T |
  Z O N E C O N T R O L ':' T H E R M O S T A T |
  Z O N E C O N T R O L ':' T H E R M O S T A T ':' O P E R A T I V E T E M P E R A T U R E |
  Z O N E C O N T R O L ':' T H E R M O S T A T ':' T H E R M A L C O M F O R T |
  Z O N E C O N T R O L ':' T H E R M O S T A T ':' T E M P E R A T U R E A N D H U M I D I T Y |
  T H E R M O S T A T S E T P O I N T ':' S I N G L E H E A T I N G |
  T H E R M O S T A T S E T P O I N T ':' S I N G L E C O O L I N G |
  T H E R M O S T A T S E T P O I N T ':' S I N G L E H E A T I N G O R C O O L I N G |
  T H E R M O S T A T S E T P O I N T ':' D U A L S E T P O I N T |
  T H E R M O S T A T S E T P O I N T ':' T H E R M A L C O M F O R T ':' F A N G E R ':' S I N G L E H E A T I N G |
  T H E R M O S T A T S E T P O I N T ':' T H E R M A L C O M F O R T ':' F A N G E R ':' S I N G L E C O O L I N G |
  T H E R M O S T A T S E T P O I N T ':' T H E R M A L C O M F O R T ':' F A N G E R ':' S I N G L E H E A T I N G O R C O O L I N G |
  T H E R M O S T A T S E T P O I N T ':' T H E R M A L C O M F O R T ':' F A N G E R ':' D U A L S E T P O I N T |
  Z O N E C O N T R O L ':' T H E R M O S T A T ':' S T A G E D D U A L S E T P O I N T |
  Z O N E C O N T R O L ':' C O N T A M I N A N T C O N T R O L L E R |
  Z O N E H V A C ':' I D E A L L O A D S A I R S Y S T E M |
  Z O N E H V A C ':' F O U R P I P E F A N C O I L |
  Z O N E H V A C ':' W I N D O W A I R C O N D I T I O N E R |
  Z O N E H V A C ':' P A C K A G E D T E R M I N A L A I R C O N D I T I O N E R |
  Z O N E H V A C ':' P A C K A G E D T E R M I N A L H E A T P U M P |
  Z O N E H V A C ':' W A T E R T O A I R H E A T P U M P |
  Z O N E H V A C ':' D E H U M I D I F I E R ':' D X |
  Z O N E H V A C ':' E N E R G Y R E C O V E R Y V E N T I L A T O R |
  Z O N E H V A C ':' E N E R G Y R E C O V E R Y V E N T I L A T O R ':' C O N T R O L L E R |
  Z O N E H V A C ':' U N I T V E N T I L A T O R |
  Z O N E H V A C ':' U N I T H E A T E R |
  Z O N E H V A C ':' E V A P O R A T I V E C O O L E R U N I T |
  Z O N E H V A C ':' H Y B R I D U N I T A R Y H V A C |
  Z O N E H V A C ':' O U T D O O R A I R U N I T |
  Z O N E H V A C ':' O U T D O O R A I R U N I T ':' E Q U I P M E N T L I S T |
  Z O N E H V A C ':' T E R M I N A L U N I T ':' V A R I A B L E R E F R I G E R A N T F L O W |
  Z O N E H V A C ':' B A S E B O A R D ':' R A D I A N T C O N V E C T I V E ':' W A T E R |
  Z O N E H V A C ':' B A S E B O A R D ':' R A D I A N T C O N V E C T I V E ':' S T E A M |
  Z O N E H V A C ':' B A S E B O A R D ':' R A D I A N T C O N V E C T I V E ':' E L E C T R I C |
  Z O N E H V A C ':' C O O L I N G P A N E L ':' R A D I A N T C O N V E C T I V E ':' W A T E R |
  Z O N E H V A C ':' B A S E B O A R D ':' C O N V E C T I V E ':' W A T E R |
  Z O N E H V A C ':' B A S E B O A R D ':' C O N V E C T I V E ':' E L E C T R I C |
  Z O N E H V A C ':' L O W T E M P E R A T U R E R A D I A N T ':' V A R I A B L E F L O W |
  Z O N E H V A C ':' L O W T E M P E R A T U R E R A D I A N T ':' C O N S T A N T F L O W |
  Z O N E H V A C ':' L O W T E M P E R A T U R E R A D I A N T ':' E L E C T R I C |
  Z O N E H V A C ':' L O W T E M P E R A T U R E R A D I A N T ':' S U R F A C E G R O U P |
  Z O N E H V A C ':' H I G H T E M P E R A T U R E R A D I A N T |
  Z O N E H V A C ':' V E N T I L A T E D S L A B |
  Z O N E H V A C ':' V E N T I L A T E D S L A B ':' S L A B G R O U P |
  A I R T E R M I N A L ':' S I N G L E D U C T ':' U N C O N T R O L L E D |
  A I R T E R M I N A L ':' S I N G L E D U C T ':' C O N S T A N T V O L U M E ':' R E H E A T |
  A I R T E R M I N A L ':' S I N G L E D U C T ':' C O N S T A N T V O L U M E ':' N O R E H E A T |
  A I R T E R M I N A L ':' S I N G L E D U C T ':' V A V ':' N O R E H E A T |
  A I R T E R M I N A L ':' S I N G L E D U C T ':' V A V ':' R E H E A T |
  A I R T E R M I N A L ':' S I N G L E D U C T ':' V A V ':' R E H E A T ':' V A R I A B L E S P E E D F A N |
  A I R T E R M I N A L ':' S I N G L E D U C T ':' V A V ':' H E A T A N D C O O L ':' N O R E H E A T |
  A I R T E R M I N A L ':' S I N G L E D U C T ':' V A V ':' H E A T A N D C O O L ':' R E H E A T |
  A I R T E R M I N A L ':' S I N G L E D U C T ':' S E R I E S P I U ':' R E H E A T |
  A I R T E R M I N A L ':' S I N G L E D U C T ':' P A R A L L E L P I U ':' R E H E A T |
  A I R T E R M I N A L ':' S I N G L E D U C T ':' C O N S T A N T V O L U M E ':' F O U R P I P E I N D U C T I O N |
  A I R T E R M I N A L ':' S I N G L E D U C T ':' C O N S T A N T V O L U M E ':' F O U R P I P E B E A M |
  A I R T E R M I N A L ':' S I N G L E D U C T ':' C O N S T A N T V O L U M E ':' C O O L E D B E A M |
  A I R T E R M I N A L ':' S I N G L E D U C T ':' M I X E R |
  A I R T E R M I N A L ':' D U A L D U C T ':' C O N S T A N T V O L U M E |
  A I R T E R M I N A L ':' D U A L D U C T ':' V A V |
  A I R T E R M I N A L ':' D U A L D U C T ':' V A V ':' O U T D O O R A I R |
  Z O N E H V A C ':' A I R D I S T R I B U T I O N U N I T |
  Z O N E H V A C ':' E Q U I P M E N T L I S T |
  Z O N E H V A C ':' E Q U I P M E N T C O N N E C T I O N S |
  F A N ':' S Y S T E M M O D E L |
  F A N ':' C O N S T A N T V O L U M E |
  F A N ':' V A R I A B L E V O L U M E |
  F A N ':' O N O F F |
  F A N ':' Z O N E E X H A U S T |
  F A N P E R F O R M A N C E ':' N I G H T V E N T I L A T I O N |
  F A N ':' C O M P O N E N T M O D E L |
  C O I L ':' C O O L I N G ':' W A T E R |
  C O I L ':' C O O L I N G ':' W A T E R ':' D E T A I L E D G E O M E T R Y |
  C O I L ':' C O O L I N G ':' D X ':' S I N G L E S P E E D |
  C O I L ':' C O O L I N G ':' D X ':' T W O S P E E D |
  C O I L ':' C O O L I N G ':' D X ':' M U L T I S P E E D |
  C O I L ':' C O O L I N G ':' D X ':' V A R I A B L E S P E E D |
  C O I L ':' C O O L I N G ':' D X ':' T W O S T A G E W I T H H U M I D I T Y C O N T R O L M O D E |
  C O I L P E R F O R M A N C E ':' D X ':' C O O L I N G |
  C O I L ':' C O O L I N G ':' D X ':' V A R I A B L E R E F R I G E R A N T F L O W |
  C O I L ':' H E A T I N G ':' D X ':' V A R I A B L E R E F R I G E R A N T F L O W |
  C O I L ':' C O O L I N G ':' D X ':' V A R I A B L E R E F R I G E R A N T F L O W ':' F L U I D T E M P E R A T U R E C O N T R O L |
  C O I L ':' H E A T I N G ':' D X ':' V A R I A B L E R E F R I G E R A N T F L O W ':' F L U I D T E M P E R A T U R E C O N T R O L |
  C O I L ':' H E A T I N G ':' W A T E R |
  C O I L ':' H E A T I N G ':' S T E A M |
  C O I L ':' H E A T I N G ':' E L E C T R I C |
  C O I L ':' H E A T I N G ':' E L E C T R I C ':' M U L T I S T A G E |
  C O I L ':' H E A T I N G ':' F U E L |
  C O I L ':' H E A T I N G ':' G A S ':' M U L T I S T A G E |
  C O I L ':' H E A T I N G ':' D E S U P E R H E A T E R |
  C O I L ':' H E A T I N G ':' D X ':' S I N G L E S P E E D |
  C O I L ':' H E A T I N G ':' D X ':' M U L T I S P E E D |
  C O I L ':' H E A T I N G ':' D X ':' V A R I A B L E S P E E D |
  C O I L ':' C O O L I N G ':' W A T E R T O A I R H E A T P U M P ':' P A R A M E T E R E S T I M A T I O N |
  C O I L ':' H E A T I N G ':' W A T E R T O A I R H E A T P U M P ':' P A R A M E T E R E S T I M A T I O N |
  C O I L ':' C O O L I N G ':' W A T E R T O A I R H E A T P U M P ':' E Q U A T I O N F I T |
  C O I L ':' C O O L I N G ':' W A T E R T O A I R H E A T P U M P ':' V A R I A B L E S P E E D E Q U A T I O N F I T |
  C O I L ':' H E A T I N G ':' W A T E R T O A I R H E A T P U M P ':' E Q U A T I O N F I T |
  C O I L ':' H E A T I N G ':' W A T E R T O A I R H E A T P U M P ':' V A R I A B L E S P E E D E Q U A T I O N F I T |
  C O I L ':' W A T E R H E A T I N G ':' A I R T O W A T E R H E A T P U M P ':' P U M P E D |
  C O I L ':' W A T E R H E A T I N G ':' A I R T O W A T E R H E A T P U M P ':' W R A P P E D |
  C O I L ':' W A T E R H E A T I N G ':' A I R T O W A T E R H E A T P U M P ':' V A R I A B L E S P E E D |
  C O I L ':' W A T E R H E A T I N G ':' D E S U P E R H E A T E R |
  C O I L S Y S T E M ':' C O O L I N G ':' D X |
  C O I L S Y S T E M ':' H E A T I N G ':' D X |
  C O I L S Y S T E M ':' C O O L I N G ':' W A T E R ':' H E A T E X C H A N G E R A S S I S T E D |
  C O I L S Y S T E M ':' C O O L I N G ':' D X ':' H E A T E X C H A N G E R A S S I S T E D |
  C O I L S Y S T E M ':' I N T E G R A T E D H E A T P U M P ':' A I R S O U R C E |
  C O I L ':' C O O L I N G ':' D X ':' S I N G L E S P E E D ':' T H E R M A L S T O R A G E |
  E V A P O R A T I V E C O O L E R ':' D I R E C T ':' C E L D E K P A D |
  E V A P O R A T I V E C O O L E R ':' I N D I R E C T ':' C E L D E K P A D |
  E V A P O R A T I V E C O O L E R ':' I N D I R E C T ':' W E T C O I L |
  E V A P O R A T I V E C O O L E R ':' I N D I R E C T ':' R E S E A R C H S P E C I A L |
  E V A P O R A T I V E C O O L E R ':' D I R E C T ':' R E S E A R C H S P E C I A L |
  H U M I D I F I E R ':' S T E A M ':' E L E C T R I C |
  H U M I D I F I E R ':' S T E A M ':' G A S |
  D E H U M I D I F I E R ':' D E S I C C A N T ':' N O F A N S |
  D E H U M I D I F I E R ':' D E S I C C A N T ':' S Y S T E M |
  H E A T E X C H A N G E R ':' A I R T O A I R ':' F L A T P L A T E |
  H E A T E X C H A N G E R ':' A I R T O A I R ':' S E N S I B L E A N D L A T E N T |
  H E A T E X C H A N G E R ':' D E S I C C A N T ':' B A L A N C E D F L O W |
  H E A T E X C H A N G E R ':' D E S I C C A N T ':' B A L A N C E D F L O W ':' P E R F O R M A N C E D A T A T Y P E '1' |
  A I R L O O P H V A C ':' U N I T A R Y S Y S T E M |
  U N I T A R Y S Y S T E M P E R F O R M A N C E ':' M U L T I S P E E D |
  A I R L O O P H V A C ':' U N I T A R Y ':' F U R N A C E ':' H E A T O N L Y |
  A I R L O O P H V A C ':' U N I T A R Y ':' F U R N A C E ':' H E A T C O O L |
  A I R L O O P H V A C ':' U N I T A R Y H E A T O N L Y |
  A I R L O O P H V A C ':' U N I T A R Y H E A T C O O L |
  A I R L O O P H V A C ':' U N I T A R Y H E A T P U M P ':' A I R T O A I R |
  A I R L O O P H V A C ':' U N I T A R Y H E A T P U M P ':' W A T E R T O A I R |
  A I R L O O P H V A C ':' U N I T A R Y H E A T C O O L ':' V A V C H A N G E O V E R B Y P A S S |
  A I R L O O P H V A C ':' U N I T A R Y H E A T P U M P ':' A I R T O A I R ':' M U L T I S P E E D |
  A I R C O N D I T I O N E R ':' V A R I A B L E R E F R I G E R A N T F L O W |
  A I R C O N D I T I O N E R ':' V A R I A B L E R E F R I G E R A N T F L O W ':' F L U I D T E M P E R A T U R E C O N T R O L |
  A I R C O N D I T I O N E R ':' V A R I A B L E R E F R I G E R A N T F L O W ':' F L U I D T E M P E R A T U R E C O N T R O L ':' H R |
  Z O N E T E R M I N A L U N I T L I S T |
  C O N T R O L L E R ':' W A T E R C O I L |
  C O N T R O L L E R ':' O U T D O O R A I R |
  C O N T R O L L E R ':' M E C H A N I C A L V E N T I L A T I O N |
  A I R L O O P H V A C ':' C O N T R O L L E R L I S T |
  A I R L O O P H V A C |
  A I R L O O P H V A C ':' O U T D O O R A I R S Y S T E M ':' E Q U I P M E N T L I S T |
  A I R L O O P H V A C ':' O U T D O O R A I R S Y S T E M |
  O U T D O O R A I R ':' M I X E R |
  A I R L O O P H V A C ':' Z O N E S P L I T T E R |
  A I R L O O P H V A C ':' S U P P L Y P L E N U M |
  A I R L O O P H V A C ':' S U P P L Y P A T H |
  A I R L O O P H V A C ':' Z O N E M I X E R |
  A I R L O O P H V A C ':' R E T U R N P L E N U M |
  A I R L O O P H V A C ':' R E T U R N P A T H |
  A I R L O O P H V A C ':' D E D I C A T E D O U T D O O R A I R S Y S T E M |
  A I R L O O P H V A C ':' M I X E R |
  A I R L O O P H V A C ':' S P L I T T E R |
  B R A N C H |
  B R A N C H L I S T |
  C O N N E C T O R ':' S P L I T T E R |
  C O N N E C T O R ':' M I X E R |
  C O N N E C T O R L I S T |
  N O D E L I S T |
  O U T D O O R A I R ':' N O D E |
  O U T D O O R A I R ':' N O D E L I S T |
  P I P E ':' A D I A B A T I C |
  P I P E ':' A D I A B A T I C ':' S T E A M |
  P I P E ':' I N D O O R |
  P I P E ':' O U T D O O R |
  P I P E ':' U N D E R G R O U N D |
  P I P I N G S Y S T E M ':' U N D E R G R O U N D ':' D O M A I N |
  P I P I N G S Y S T E M ':' U N D E R G R O U N D ':' P I P E C I R C U I T |
  P I P I N G S Y S T E M ':' U N D E R G R O U N D ':' P I P E S E G M E N T |
  D U C T |
  P U M P ':' V A R I A B L E S P E E D |
  P U M P ':' C O N S T A N T S P E E D |
  P U M P ':' V A R I A B L E S P E E D ':' C O N D E N S A T E |
  H E A D E R E D P U M P S ':' C O N S T A N T S P E E D |
  H E A D E R E D P U M P S ':' V A R I A B L E S P E E D |
  T E M P E R I N G V A L V E |
  L O A D P R O F I L E ':' P L A N T |
  S O L A R C O L L E C T O R P E R F O R M A N C E ':' F L A T P L A T E |
  S O L A R C O L L E C T O R ':' F L A T P L A T E ':' W A T E R |
  S O L A R C O L L E C T O R ':' F L A T P L A T E ':' P H O T O V O L T A I C T H E R M A L |
  S O L A R C O L L E C T O R P E R F O R M A N C E ':' P H O T O V O L T A I C T H E R M A L ':' S I M P L E |
  S O L A R C O L L E C T O R ':' I N T E G R A L C O L L E C T O R S T O R A G E |
  S O L A R C O L L E C T O R P E R F O R M A N C E ':' I N T E G R A L C O L L E C T O R S T O R A G E |
  S O L A R C O L L E C T O R ':' U N G L A Z E D T R A N S P I R E D |
  S O L A R C O L L E C T O R ':' U N G L A Z E D T R A N S P I R E D ':' M U L T I S Y S T E M |
  B O I L E R ':' H O T W A T E R |
  B O I L E R ':' S T E A M |
  C H I L L E R ':' E L E C T R I C ':' E I R |
  C H I L L E R ':' E L E C T R I C ':' R E F O R M U L A T E D E I R |
  C H I L L E R ':' E L E C T R I C |
  C H I L L E R ':' A B S O R P T I O N ':' I N D I R E C T |
  C H I L L E R ':' A B S O R P T I O N |
  C H I L L E R ':' C O N S T A N T C O P |
  C H I L L E R ':' E N G I N E D R I V E N |
  C H I L L E R ':' C O M B U S T I O N T U R B I N E |
  C H I L L E R H E A T E R ':' A B S O R P T I O N ':' D I R E C T F I R E D |
  C H I L L E R H E A T E R ':' A B S O R P T I O N ':' D O U B L E E F F E C T |
  H E A T P U M P ':' W A T E R T O W A T E R ':' E I R ':' C O O L I N G |
  H E A T P U M P ':' W A T E R T O W A T E R ':' E I R ':' H E A T I N G |
  H E A T P U M P ':' W A T E R T O W A T E R ':' E Q U A T I O N F I T ':' H E A T I N G |
  H E A T P U M P ':' W A T E R T O W A T E R ':' E Q U A T I O N F I T ':' C O O L I N G |
  H E A T P U M P ':' W A T E R T O W A T E R ':' P A R A M E T E R E S T I M A T I O N ':' C O O L I N G |
  H E A T P U M P ':' W A T E R T O W A T E R ':' P A R A M E T E R E S T I M A T I O N ':' H E A T I N G |
  D I S T R I C T C O O L I N G |
  D I S T R I C T H E A T I N G |
  P L A N T C O M P O N E N T ':' T E M P E R A T U R E S O U R C E |
  C E N T R A L H E A T P U M P S Y S T E M |
  C H I L L E R H E A T E R P E R F O R M A N C E ':' E L E C T R I C ':' E I R |
  C O O L I N G T O W E R ':' S I N G L E S P E E D |
  C O O L I N G T O W E R ':' T W O S P E E D |
  C O O L I N G T O W E R ':' V A R I A B L E S P E E D ':' M E R K E L |
  C O O L I N G T O W E R ':' V A R I A B L E S P E E D |
  C O O L I N G T O W E R P E R F O R M A N C E ':' C O O L T O O L S |
  C O O L I N G T O W E R P E R F O R M A N C E ':' Y O R K C A L C |
  E V A P O R A T I V E F L U I D C O O L E R ':' S I N G L E S P E E D |
  E V A P O R A T I V E F L U I D C O O L E R ':' T W O S P E E D |
  F L U I D C O O L E R ':' S I N G L E S P E E D |
  F L U I D C O O L E R ':' T W O S P E E D |
  G R O U N D H E A T E X C H A N G E R ':' S Y S T E M |
  G R O U N D H E A T E X C H A N G E R ':' V E R T I C A L ':' P R O P E R T I E S |
  G R O U N D H E A T E X C H A N G E R ':' V E R T I C A L ':' A R R A Y |
  G R O U N D H E A T E X C H A N G E R ':' V E R T I C A L ':' S I N G L E |
  G R O U N D H E A T E X C H A N G E R ':' R E S P O N S E F A C T O R S |
  G R O U N D H E A T E X C H A N G E R ':' P O N D |
  G R O U N D H E A T E X C H A N G E R ':' S U R F A C E |
  G R O U N D H E A T E X C H A N G E R ':' H O R I Z O N T A L T R E N C H |
  G R O U N D H E A T E X C H A N G E R ':' S L I N K Y |
  H E A T E X C H A N G E R ':' F L U I D T O F L U I D |
  W A T E R H E A T E R ':' M I X E D |
  W A T E R H E A T E R ':' S T R A T I F I E D |
  W A T E R H E A T E R ':' S I Z I N G |
  W A T E R H E A T E R ':' H E A T P U M P ':' P U M P E D C O N D E N S E R |
  W A T E R H E A T E R ':' H E A T P U M P ':' W R A P P E D C O N D E N S E R |
  T H E R M A L S T O R A G E ':' I C E ':' S I M P L E |
  T H E R M A L S T O R A G E ':' I C E ':' D E T A I L E D |
  T H E R M A L S T O R A G E ':' C H I L L E D W A T E R ':' M I X E D |
  T H E R M A L S T O R A G E ':' C H I L L E D W A T E R ':' S T R A T I F I E D |
  P L A N T L O O P |
  C O N D E N S E R L O O P |
  P L A N T E Q U I P M E N T L I S T |
  C O N D E N S E R E Q U I P M E N T L I S T |
  P L A N T E Q U I P M E N T O P E R A T I O N ':' U N C O N T R O L L E D |
  P L A N T E Q U I P M E N T O P E R A T I O N ':' C O O L I N G L O A D |
  P L A N T E Q U I P M E N T O P E R A T I O N ':' H E A T I N G L O A D |
  P L A N T E Q U I P M E N T O P E R A T I O N ':' O U T D O O R D R Y B U L B |
  P L A N T E Q U I P M E N T O P E R A T I O N ':' O U T D O O R W E T B U L B |
  P L A N T E Q U I P M E N T O P E R A T I O N ':' O U T D O O R R E L A T I V E H U M I D I T Y |
  P L A N T E Q U I P M E N T O P E R A T I O N ':' O U T D O O R D E W P O I N T |
  P L A N T E Q U I P M E N T O P E R A T I O N ':' C O M P O N E N T S E T P O I N T |
  P L A N T E Q U I P M E N T O P E R A T I O N ':' T H E R M A L E N E R G Y S T O R A G E |
  P L A N T E Q U I P M E N T O P E R A T I O N ':' O U T D O O R D R Y B U L B D I F F E R E N C E |
  P L A N T E Q U I P M E N T O P E R A T I O N ':' O U T D O O R W E T B U L B D I F F E R E N C E |
  P L A N T E Q U I P M E N T O P E R A T I O N ':' O U T D O O R D E W P O I N T D I F F E R E N C E |
  P L A N T E Q U I P M E N T O P E R A T I O N S C H E M E S |
  C O N D E N S E R E Q U I P M E N T O P E R A T I O N S C H E M E S |
  E N E R G Y M A N A G E M E N T S Y S T E M ':' S E N S O R |
  E N E R G Y M A N A G E M E N T S Y S T E M ':' A C T U A T O R |
  E N E R G Y M A N A G E M E N T S Y S T E M ':' P R O G R A M C A L L I N G M A N A G E R |
  E N E R G Y M A N A G E M E N T S Y S T E M ':' P R O G R A M |
  E N E R G Y M A N A G E M E N T S Y S T E M ':' S U B R O U T I N E |
  E N E R G Y M A N A G E M E N T S Y S T E M ':' G L O B A L V A R I A B L E |
  E N E R G Y M A N A G E M E N T S Y S T E M ':' O U T P U T V A R I A B L E |
  E N E R G Y M A N A G E M E N T S Y S T E M ':' M E T E R E D O U T P U T V A R I A B L E |
  E N E R G Y M A N A G E M E N T S Y S T E M ':' T R E N D V A R I A B L E |
  E N E R G Y M A N A G E M E N T S Y S T E M ':' I N T E R N A L V A R I A B L E |
  E N E R G Y M A N A G E M E N T S Y S T E M ':' C U R V E O R T A B L E I N D E X V A R I A B L E |
  E N E R G Y M A N A G E M E N T S Y S T E M ':' C O N S T R U C T I O N I N D E X V A R I A B L E |
  E X T E R N A L I N T E R F A C E |
  E X T E R N A L I N T E R F A C E ':' S C H E D U L E |
  E X T E R N A L I N T E R F A C E ':' V A R I A B L E |
  E X T E R N A L I N T E R F A C E ':' A C T U A T O R |
  E X T E R N A L I N T E R F A C E ':' F U N C T I O N A L M O C K U P U N I T I M P O R T |
  E X T E R N A L I N T E R F A C E ':' F U N C T I O N A L M O C K U P U N I T I M P O R T ':' F R O M ':' V A R I A B L E |
  E X T E R N A L I N T E R F A C E ':' F U N C T I O N A L M O C K U P U N I T I M P O R T ':' T O ':' S C H E D U L E |
  E X T E R N A L I N T E R F A C E ':' F U N C T I O N A L M O C K U P U N I T I M P O R T ':' T O ':' A C T U A T O R |
  E X T E R N A L I N T E R F A C E ':' F U N C T I O N A L M O C K U P U N I T I M P O R T ':' T O ':' V A R I A B L E |
  E X T E R N A L I N T E R F A C E ':' F U N C T I O N A L M O C K U P U N I T E X P O R T ':' F R O M ':' V A R I A B L E |
  E X T E R N A L I N T E R F A C E ':' F U N C T I O N A L M O C K U P U N I T E X P O R T ':' T O ':' S C H E D U L E |
  E X T E R N A L I N T E R F A C E ':' F U N C T I O N A L M O C K U P U N I T E X P O R T ':' T O ':' A C T U A T O R |
  E X T E R N A L I N T E R F A C E ':' F U N C T I O N A L M O C K U P U N I T E X P O R T ':' T O ':' V A R I A B L E |
  Z O N E H V A C ':' F O R C E D A I R ':' U S E R D E F I N E D |
  A I R T E R M I N A L ':' S I N G L E D U C T ':' U S E R D E F I N E D |
  C O I L ':' U S E R D E F I N E D |
  P L A N T C O M P O N E N T ':' U S E R D E F I N E D |
  P L A N T E Q U I P M E N T O P E R A T I O N ':' U S E R D E F I N E D |
  A V A I L A B I L I T Y M A N A G E R ':' S C H E D U L E D |
  A V A I L A B I L I T Y M A N A G E R ':' S C H E D U L E D O N |
  A V A I L A B I L I T Y M A N A G E R ':' S C H E D U L E D O F F |
  A V A I L A B I L I T Y M A N A G E R ':' O P T I M U M S T A R T |
  A V A I L A B I L I T Y M A N A G E R ':' N I G H T C Y C L E |
  A V A I L A B I L I T Y M A N A G E R ':' D I F F E R E N T I A L T H E R M O S T A T |
  A V A I L A B I L I T Y M A N A G E R ':' H I G H T E M P E R A T U R E T U R N O F F |
  A V A I L A B I L I T Y M A N A G E R ':' H I G H T E M P E R A T U R E T U R N O N |
  A V A I L A B I L I T Y M A N A G E R ':' L O W T E M P E R A T U R E T U R N O F F |
  A V A I L A B I L I T Y M A N A G E R ':' L O W T E M P E R A T U R E T U R N O N |
  A V A I L A B I L I T Y M A N A G E R ':' N I G H T V E N T I L A T I O N |
  A V A I L A B I L I T Y M A N A G E R ':' H Y B R I D V E N T I L A T I O N |
  A V A I L A B I L I T Y M A N A G E R A S S I G N M E N T L I S T |
  S E T P O I N T M A N A G E R ':' S C H E D U L E D |
  S E T P O I N T M A N A G E R ':' S C H E D U L E D ':' D U A L S E T P O I N T |
  S E T P O I N T M A N A G E R ':' O U T D O O R A I R R E S E T |
  S E T P O I N T M A N A G E R ':' S I N G L E Z O N E ':' R E H E A T |
  S E T P O I N T M A N A G E R ':' S I N G L E Z O N E ':' H E A T I N G |
  S E T P O I N T M A N A G E R ':' S I N G L E Z O N E ':' C O O L I N G |
  S E T P O I N T M A N A G E R ':' S I N G L E Z O N E ':' H U M I D I T Y ':' M I N I M U M |
  S E T P O I N T M A N A G E R ':' S I N G L E Z O N E ':' H U M I D I T Y ':' M A X I M U M |
  S E T P O I N T M A N A G E R ':' M I X E D A I R |
  S E T P O I N T M A N A G E R ':' O U T D O O R A I R P R E T R E A T |
  S E T P O I N T M A N A G E R ':' W A R M E S T |
  S E T P O I N T M A N A G E R ':' C O L D E S T |
  S E T P O I N T M A N A G E R ':' R E T U R N A I R B Y P A S S F L O W |
  S E T P O I N T M A N A G E R ':' W A R M E S T T E M P E R A T U R E F L O W |
  S E T P O I N T M A N A G E R ':' M U L T I Z O N E ':' H E A T I N G ':' A V E R A G E |
  S E T P O I N T M A N A G E R ':' M U L T I Z O N E ':' C O O L I N G ':' A V E R A G E |
  S E T P O I N T M A N A G E R ':' M U L T I Z O N E ':' M I N I M U M H U M I D I T Y ':' A V E R A G E |
  S E T P O I N T M A N A G E R ':' M U L T I Z O N E ':' M A X I M U M H U M I D I T Y ':' A V E R A G E |
  S E T P O I N T M A N A G E R ':' M U L T I Z O N E ':' H U M I D I T Y ':' M I N I M U M |
  S E T P O I N T M A N A G E R ':' M U L T I Z O N E ':' H U M I D I T Y ':' M A X I M U M |
  S E T P O I N T M A N A G E R ':' F O L L O W O U T D O O R A I R T E M P E R A T U R E |
  S E T P O I N T M A N A G E R ':' F O L L O W S Y S T E M N O D E T E M P E R A T U R E |
  S E T P O I N T M A N A G E R ':' F O L L O W G R O U N D T E M P E R A T U R E |
  S E T P O I N T M A N A G E R ':' C O N D E N S E R E N T E R I N G R E S E T |
  S E T P O I N T M A N A G E R ':' C O N D E N S E R E N T E R I N G R E S E T ':' I D E A L |
  S E T P O I N T M A N A G E R ':' S I N G L E Z O N E ':' O N E S T A G E C O O L I N G |
  S E T P O I N T M A N A G E R ':' S I N G L E Z O N E ':' O N E S T A G E H E A T I N G |
  S E T P O I N T M A N A G E R ':' R E T U R N T E M P E R A T U R E ':' C H I L L E D W A T E R |
  S E T P O I N T M A N A G E R ':' R E T U R N T E M P E R A T U R E ':' H O T W A T E R |
  R E F R I G E R A T I O N ':' C A S E |
  R E F R I G E R A T I O N ':' C O M P R E S S O R R A C K |
  R E F R I G E R A T I O N ':' C A S E A N D W A L K I N L I S T |
  R E F R I G E R A T I O N ':' C O N D E N S E R ':' A I R C O O L E D |
  R E F R I G E R A T I O N ':' C O N D E N S E R ':' E V A P O R A T I V E C O O L E D |
  R E F R I G E R A T I O N ':' C O N D E N S E R ':' W A T E R C O O L E D |
  R E F R I G E R A T I O N ':' C O N D E N S E R ':' C A S C A D E |
  R E F R I G E R A T I O N ':' G A S C O O L E R ':' A I R C O O L E D |
  R E F R I G E R A T I O N ':' T R A N S F E R L O A D L I S T |
  R E F R I G E R A T I O N ':' S U B C O O L E R |
  R E F R I G E R A T I O N ':' C O M P R E S S O R |
  R E F R I G E R A T I O N ':' C O M P R E S S O R L I S T |
  R E F R I G E R A T I O N ':' S Y S T E M |
  R E F R I G E R A T I O N ':' T R A N S C R I T I C A L S Y S T E M |
  R E F R I G E R A T I O N ':' S E C O N D A R Y S Y S T E M |
  R E F R I G E R A T I O N ':' W A L K I N |
  R E F R I G E R A T I O N ':' A I R C H I L L E R |
  Z O N E H V A C ':' R E F R I G E R A T I O N C H I L L E R S E T |
  D E M A N D M A N A G E R A S S I G N M E N T L I S T |
  D E M A N D M A N A G E R ':' E X T E R I O R L I G H T S |
  D E M A N D M A N A G E R ':' L I G H T S |
  D E M A N D M A N A G E R ':' E L E C T R I C E Q U I P M E N T |
  D E M A N D M A N A G E R ':' T H E R M O S T A T S |
  D E M A N D M A N A G E R ':' V E N T I L A T I O N |
  G E N E R A T O R ':' I N T E R N A L C O M B U S T I O N E N G I N E |
  G E N E R A T O R ':' C O M B U S T I O N T U R B I N E |
  G E N E R A T O R ':' M I C R O T U R B I N E |
  G E N E R A T O R ':' P H O T O V O L T A I C |
  P H O T O V O L T A I C P E R F O R M A N C E ':' S I M P L E |
  P H O T O V O L T A I C P E R F O R M A N C E ':' E Q U I V A L E N T O N E '-' D I O D E |
  P H O T O V O L T A I C P E R F O R M A N C E ':' S A N D I A |
  G E N E R A T O R ':' P V W A T T S |
  E L E C T R I C L O A D C E N T E R ':' I N V E R T E R ':' P V W A T T S |
  G E N E R A T O R ':' F U E L C E L L |
  G E N E R A T O R ':' F U E L C E L L ':' P O W E R M O D U L E |
  G E N E R A T O R ':' F U E L C E L L ':' A I R S U P P L Y |
  G E N E R A T O R ':' F U E L C E L L ':' W A T E R S U P P L Y |
  G E N E R A T O R ':' F U E L C E L L ':' A U X I L I A R Y H E A T E R |
  G E N E R A T O R ':' F U E L C E L L ':' E X H A U S T G A S T O W A T E R H E A T E X C H A N G E R |
  G E N E R A T O R ':' F U E L C E L L ':' E L E C T R I C A L S T O R A G E |
  G E N E R A T O R ':' F U E L C E L L ':' I N V E R T E R |
  G E N E R A T O R ':' F U E L C E L L ':' S T A C K C O O L E R |
  G E N E R A T O R ':' M I C R O C H P |
  G E N E R A T O R ':' M I C R O C H P ':' N O N N O R M A L I Z E D P A R A M E T E R S |
  G E N E R A T O R ':' F U E L S U P P L Y |
  G E N E R A T O R ':' W I N D T U R B I N E |
  E L E C T R I C L O A D C E N T E R ':' G E N E R A T O R S |
  E L E C T R I C L O A D C E N T E R ':' I N V E R T E R ':' S I M P L E |
  E L E C T R I C L O A D C E N T E R ':' I N V E R T E R ':' F U N C T I O N O F P O W E R |
  E L E C T R I C L O A D C E N T E R ':' I N V E R T E R ':' L O O K U P T A B L E |
  E L E C T R I C L O A D C E N T E R ':' S T O R A G E ':' S I M P L E |
  E L E C T R I C L O A D C E N T E R ':' S T O R A G E ':' B A T T E R Y |
  E L E C T R I C L O A D C E N T E R ':' T R A N S F O R M E R |
  E L E C T R I C L O A D C E N T E R ':' D I S T R I B U T I O N |
  E L E C T R I C L O A D C E N T E R ':' S T O R A G E ':' C O N V E R T E R |
  W A T E R U S E ':' E Q U I P M E N T |
  W A T E R U S E ':' C O N N E C T I O N S |
  W A T E R U S E ':' S T O R A G E |
  W A T E R U S E ':' W E L L |
  W A T E R U S E ':' R A I N C O L L E C T O R |
  F A U L T M O D E L ':' T E M P E R A T U R E S E N S O R O F F S E T ':' O U T D O O R A I R |
  F A U L T M O D E L ':' H U M I D I T Y S E N S O R O F F S E T ':' O U T D O O R A I R |
  F A U L T M O D E L ':' E N T H A L P Y S E N S O R O F F S E T ':' O U T D O O R A I R |
  F A U L T M O D E L ':' T E M P E R A T U R E S E N S O R O F F S E T ':' R E T U R N A I R |
  F A U L T M O D E L ':' E N T H A L P Y S E N S O R O F F S E T ':' R E T U R N A I R |
  F A U L T M O D E L ':' T E M P E R A T U R E S E N S O R O F F S E T ':' C H I L L E R S U P P L Y W A T E R |
  F A U L T M O D E L ':' T E M P E R A T U R E S E N S O R O F F S E T ':' C O I L S U P P L Y A I R |
  F A U L T M O D E L ':' T E M P E R A T U R E S E N S O R O F F S E T ':' C O N D E N S E R S U P P L Y W A T E R |
  F A U L T M O D E L ':' T H E R M O S T A T O F F S E T |
  F A U L T M O D E L ':' H U M I D I S T A T O F F S E T |
  F A U L T M O D E L ':' F O U L I N G ':' A I R F I L T E R |
  F A U L T M O D E L ':' F O U L I N G ':' B O I L E R |
  F A U L T M O D E L ':' F O U L I N G ':' E V A P O R A T I V E C O O L E R |
  F A U L T M O D E L ':' F O U L I N G ':' C H I L L E R |
  F A U L T M O D E L ':' F O U L I N G ':' C O O L I N G T O W E R |
  F A U L T M O D E L ':' F O U L I N G ':' C O I L |
  M A T R I X ':' T W O D I M E N S I O N |
  H Y B R I D M O D E L ':' Z O N E |
  C U R V E ':' L I N E A R |
  C U R V E ':' Q U A D L I N E A R |
  C U R V E ':' Q U A D R A T I C |
  C U R V E ':' C U B I C |
  C U R V E ':' Q U A R T I C |
  C U R V E ':' E X P O N E N T |
  C U R V E ':' B I C U B I C |
  C U R V E ':' B I Q U A D R A T I C |
  C U R V E ':' Q U A D R A T I C L I N E A R |
  C U R V E ':' C U B I C L I N E A R |
  C U R V E ':' T R I Q U A D R A T I C |
  C U R V E ':' F U N C T I O N A L ':' P R E S S U R E D R O P |
  C U R V E ':' F A N P R E S S U R E R I S E |
  C U R V E ':' E X P O N E N T I A L S K E W N O R M A L |
  C U R V E ':' S I G M O I D |
  C U R V E ':' R E C T A N G U L A R H Y P E R B O L A '1' |
  C U R V E ':' R E C T A N G U L A R H Y P E R B O L A '2' |
  C U R V E ':' E X P O N E N T I A L D E C A Y |
  C U R V E ':' D O U B L E E X P O N E N T I A L D E C A Y |
  C U R V E ':' C H I L L E R P A R T L O A D W I T H L I F T |
  T A B L E ':' I N D E P E N D E N T V A R I A B L E |
  T A B L E ':' I N D E P E N D E N T V A R I A B L E L I S T |
  T A B L E ':' L O O K U P |
  F L U I D P R O P E R T I E S ':' N A M E |
  F L U I D P R O P E R T I E S ':' G L Y C O L C O N C E N T R A T I O N |
  F L U I D P R O P E R T I E S ':' T E M P E R A T U R E S |
  F L U I D P R O P E R T I E S ':' S A T U R A T E D |
  F L U I D P R O P E R T I E S ':' S U P E R H E A T E D |
  F L U I D P R O P E R T I E S ':' C O N C E N T R A T I O N |
  C U R R E N C Y T Y P E |
  C O M P O N E N T C O S T ':' A D J U S T M E N T S |
  C O M P O N E N T C O S T ':' R E F E R E N C E |
  C O M P O N E N T C O S T ':' L I N E I T E M |
  U T I L I T Y C O S T ':' T A R I F F |
  U T I L I T Y C O S T ':' Q U A L I F Y |
  U T I L I T Y C O S T ':' C H A R G E ':' S I M P L E |
  U T I L I T Y C O S T ':' C H A R G E ':' B L O C K |
  U T I L I T Y C O S T ':' R A T C H E T |
  U T I L I T Y C O S T ':' V A R I A B L E |
  U T I L I T Y C O S T ':' C O M P U T A T I O N |
  L I F E C Y C L E C O S T ':' P A R A M E T E R S |
  L I F E C Y C L E C O S T ':' R E C U R R I N G C O S T S |
  L I F E C Y C L E C O S T ':' N O N R E C U R R I N G C O S T |
  L I F E C Y C L E C O S T ':' U S E P R I C E E S C A L A T I O N |
  L I F E C Y C L E C O S T ':' U S E A D J U S T M E N T |
  P A R A M E T R I C ':' S E T V A L U E F O R R U N |
  P A R A M E T R I C ':' L O G I C |
  P A R A M E T R I C ':' R U N C O N T R O L |
  P A R A M E T R I C ':' F I L E N A M E S U F F I X |
  O U T P U T ':' V A R I A B L E D I C T I O N A R Y |
  O U T P U T ':' S U R F A C E S ':' L I S T |
  O U T P U T ':' S U R F A C E S ':' D R A W I N G |
  O U T P U T ':' S C H E D U L E S |
  O U T P U T ':' C O N S T R U C T I O N S |
  O U T P U T ':' E N E R G Y M A N A G E M E N T S Y S T E M |
  O U T P U T C O N T R O L ':' S U R F A C E C O L O R S C H E M E |
  O U T P U T ':' T A B L E ':' S U M M A R Y R E P O R T S |
  O U T P U T ':' T A B L E ':' T I M E B I N S |
  O U T P U T ':' T A B L E ':' M O N T H L Y |
  O U T P U T ':' T A B L E ':' A N N U A L |
  O U T P U T C O N T R O L ':' T A B L E ':' S T Y L E |
  O U T P U T C O N T R O L ':' R E P O R T I N G T O L E R A N C E S |
  O U T P U T ':' V A R I A B L E |
  O U T P U T ':' M E T E R |
  O U T P U T ':' M E T E R ':' M E T E R F I L E O N L Y |
  O U T P U T ':' M E T E R ':' C U M U L A T I V E |
  O U T P U T ':' M E T E R ':' C U M U L A T I V E ':' M E T E R F I L E O N L Y |
  M E T E R ':' C U S T O M |
  M E T E R ':' C U S T O M D E C R E M E N T |
  O U T P U T ':' J S O N |
  O U T P U T ':' S Q L I T E |
  O U T P U T ':' E N V I R O N M E N T A L I M P A C T F A C T O R S |
  E N V I R O N M E N T A L I M P A C T F A C T O R S |
  F U E L F A C T O R S |
  O U T P U T ':' D I A G N O S T I C S |
  O U T P U T ':' D E B U G G I N G D A T A |
  O U T P U T ':' P R E P R O C E S S O R M E S S A G E ;
