
# WordWorld

WordWorld is a web-based game made in ASP.NET Core in the backend and React in the frontend. The game is a basically an .io game - there can be
many players playing at once through the web browser. The concept of the game itself is based on Scrabble - the player is presented with a grid,
in which they can build words from the letters they have gotten. Unlike Scrabble, players don't take turns, instead
they can create the words whenever they want - since this is supposed to be a massively multiplayer game, it would take forever to wait for one's
turn, if there were, say, 100 players.

I created this game because I thought it was an interesting concept, but also to learn how to use React.

## How to use

The game allows multiple boards to exist, and each player session is bound to one of these boards.

You can join an English board on the following endpoints: `/play` or `/play/english1`

You can also join a Czech board on the following endpoint: `/play/czech1`

In the future, I will be adding a homepage, which will make joining games way more intuitive.

## Technical stuff

### Client-server communication

The communication between the client's browser and the web server is based on restful ajax requests. This includes fetching the current state of the board -
the client downloads data about the currently viewed section of the game board every 0.5s. Admittedly, this is not the most efficient way of doing things and with
many players playing at once, this will cause a very high frequency of requests on the server. I haven't tested it with many players, so I don't know how 
many players at once the server can handle. If the throughput of requests turns out to be a problem, an alternative approach could be taken, for example
by having the server only send info about changes on the board, once they occur, to the client - this could with `EventSource` (content-type `text/event-stream`) or with `WebSockets`.

### Word checking

For each supported language, the server has a text file with all the valid words from that language, separated with newlines. During initializatin, the server loads these
words into a trie data structure to allow efficient checking of whether a word is in the set or not.

## TODO

- Replace jQuery ajax requests with fetch api
- Make translations of different languages
- Make words dissapear after either some time or add a limit to how many words can be on the board, removing the oldest words in FIFO order)
- Add homepage
- Add player usernames
- Add leaderboard