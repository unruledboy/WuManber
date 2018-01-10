# WuManber
WuManber implementation using c#.

Wu/Manber is a very high performance string/text search/match algorithm.

Wu-Manber stand on the shoulders of their Multi Pattern Search predecessors: Aho and Corasick (with a linear time scanner based upon an automata approach), Commentz-Walter (who combined Aho-Corasick with the Boyer-Moore string search algorithm), and Baeza-Yates (with a slightly different combination of Aho-Corasick and Boyer-Moore-Horspool).

This implementation is based on the C++ version from here: http://blog.raymond.burkholder.net/index.php?/archives/362-C++-Implementation-of-Wu-Manbers-Multi-Pattern-Search-Algorithm.html

PPT explaination of the algorithm is here: https://www.slideshare.net/mailund/wu-mamber-string-algorithms-2007


# Usage
The usage is very straight forward, please refer to the FunctionTest() in the Program.cs file.

Besides a list of words, you can pass in additional info like Id and Tag, just in case you would like to cross referene with your real world business.


#中文

Wu/Manber是非常高效的字符串/文本搜索/查询/匹配算法。
