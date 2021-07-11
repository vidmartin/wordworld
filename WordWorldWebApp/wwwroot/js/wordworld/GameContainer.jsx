
import { GameMenu } from "./GameMenu.jsx";
import { GameBoard } from "./GameBoard.jsx";
import { MessageBoard } from "./MessageBoard.jsx";
import { Leaderboard } from "./Leaderboard.jsx";
import { AmbiguousJokerResolvePopup } from "./AmbiguousJokerResolvePopup.jsx";
import { getCelebratoryStatementFromScore } from "./eulogies.js";

export class GameContainer extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            game: GAME_INITIAL,
            ambiguousJokerUrl: null,
            ambiguousJokerOptions: []
        };

        this.cache = {
            fetchingStatusRequest: null,
            isInitiatingFetchStatus: false
        };

        this.board = React.createRef();

        this.fetchStatus = this.fetchStatus.bind(this);
        this.fetchStatusInner = this.fetchStatusUnsafe.bind(this);
        this.handleLetterSlideIn = this.handleLetterSlideIn.bind(this);
        this.handleLetterDrag = this.handleLetterDrag.bind(this);
        this.handleLetterDrop = this.handleLetterDrop.bind(this);
        this.handleDraggedLetterSet = this.handleDraggedLetterSet.bind(this);
        this.handleWordConfirm = this.handleWordConfirm.bind(this);
        this.handleWordCancel = this.handleWordCancel.bind(this);
        this.sendWriteWordRequest = this.sendWriteWordRequest.bind(this);
        this.handleAmbiguousJokerResolve = this.handleAmbiguousJokerResolve.bind(this);
        this.handleAmbiguousJokerResolveCancel = this.handleAmbiguousJokerResolveCancel.bind(this);

        this.abortController = new AbortController(); // used to cancel fetch requests
    }

    componentDidMount() {
        // this.fetchStatus(); 
    }

    async fetchStatus() {
        if (this.cache.isInitiatingFetchStatus == true) {
            // just to be safe
            return;
        }

        this.cache.isInitiatingFetchStatus = true;

        // we don't wanna be fetching status multiple times at once
        if (this.cache.fetchingStatusRequest != null) {
            this.abortController.abort();
            await this.cache.fetchingStatusRequest.catch(() => { });
        }

        this.cache.fetchingStatusRequest = this.fetchStatusUnsafe()
            .finally(() => this.cache.fetchingStatusRequest = null);

        this.cache.isInitiatingFetchStatus = false;
    }

    // fetch player's score & inventory and update state accordingly
    // (získat info o hráčově skóre & inventáři a updatovat podle toho stav)
    // newLetters is the amount of letters that were replaced - this is used to animate the newly drawn letters
    // (newLetters je počet písmenek, která byla vyměněna - to je použito pro animaci nově vylosovaných písmen)
    // ((český překlad uveden v zájmu jednoznačnosti, anžto po anglicku není jasné jestli 'drawn' znamená 'nakreslený' nebo 'vylosovaný'))
    async fetchStatusUnsafe() {
        let oldLetters = this.state.game.inventory.length - this.state.game.usedLetterKeys.length + 1; // how many letters will stay

        let response = await fetch(`/game/status?token=${PLAYER_TOKEN}`, { signal: this.abortController.signal });

        if (response.ok != true) {
            console.error(`couldn't fetch status - http error '${response.statusText}'`);
            return;
        }

        let json = await response.json();

        if (json.status != "ok") {
            console.error(`couldn't fetch status - api error '${json.status}'`);
            return;
        }

        let biggestCurrentKey = this.state.game.inventory.reduce((prev, curr) => Math.max(prev, curr.key), 0);

        let letterArray = Array.from(json.data.inventory); // array from string
        let minNewIndex = oldLetters - 1;
        letterArray = letterArray
            .filter((_, i) => i >= minNewIndex)
            .map((ch, i) => ({ key: biggestCurrentKey + i + 1, letter: ch }));

        this.setState({
            game: {
                ...this.state.game,
                score: json.data.score,
                inventory: this.state.game.inventory
                    .filter(item => this.state.game.usedLetterKeys.indexOf(item.key) == -1)
                    .concat(letterArray),
                lettersToAnimate: letterArray.length,
                usedLetterKeys: []
            }
        });

        // this.cache.unfetchedNewLetters = 0;
    }

    handleLetterSlideIn() {
        this.setState({
            game: {
                ...this.state.game,
                lettersToAnimate: Math.max(0, this.state.game.lettersToAnimate - 1)
            }
        });
    }

    handleLetterDrag(mousePos) {

    }

    handleLetterDrop(mousePos) {
        if (this.board.current.dropLetter() == true) {
            this.setState({
                game: {
                    ...this.state.game,
                    usedLetterKeys: this.state.game.usedLetterKeys.concat([this.state.game.currentlyDraggedLetter.key])
                }
            });            

            return true;
        }

        return false;
    }

    handleDraggedLetterSet(draggedLetterKey) {
        if (draggedLetterKey == null) {
            this.setState({
                game: {
                    ...this.state.game,
                    currentlyDraggedLetter: null
                }
            });
        } else {
            let match = this.state.game.inventory.find(l => l.key == draggedLetterKey); // find object of letter by key

            this.setState({
                game: {
                    ...this.state.game,
                    currentlyDraggedLetter: match // (passed by reference)
                }
            });
        }
    }

    handleWordConfirm() {
        let word = this.board.current.getWordSafeDir();

        // get the indexes of letters that were used in the new word
        let usedIndices = this.state.game.usedLetterKeys.map(val1 =>
            this.state.game.inventory.findIndex(val2 => val2.key == val1));

        let url = `/game/write?token=${PLAYER_TOKEN}&word=${word.wordStr}&direction=${word.wordDir}&x=${word.wordPos.x}&y=${word.wordPos.y}&used=${usedIndices.join("_")}`;
        this.sendWriteWordRequest(url);
    }

    async sendWriteWordRequest(url) {
        let response = await fetch(url);

        if (response.ok != true) {
            console.error(`couldn't write word - http error '${response.statusText}'`);
            return;
        }

        let json = await response.json();

        console.log(`writing word - received json: ${json}`);

        if (json.status != "ok") {
            console.warn(`couldn't write word - api error '${json.status}'`);

            if (json.status == "ambiguous_joker") {
                // if there is a case of the ambiguous joker, set the state this way to trigger the ambiguous joker resolving menu
                console.log("ambiguous joker!!");                

                this.setState({
                    ambiguousJokerUrl: url,
                    ambiguousJokerOptions: json.data.possibilities
                });

                return;
            }

            if (json.status in STATUS_ERRORS) {
                MessageBoard.writeError(STATUS_ERRORS[json.status]);
            } else {
                MessageBoard.writeError(`[${json.status}]`);
            }
            
            return; // backend isn't happy
        }

        let deltaScore = json.data.score - this.state.game.score;

        this.setState({
            game: {
                ...this.state.game,
                score: json.data.score
            }
        });

        MessageBoard.writeOk(getCelebratoryStatementFromScore(deltaScore));

        console.log(":)");
        this.board.current.fetchBoard();
        this.board.current.clearWord();
        this.fetchStatus();
    }

    handleWordCancel() {
        this.board.current.clearWord();
        this.setState({
            game: {
                ...this.state.game,
                usedLetterKeys: []
            }
        });
    }

    handleAmbiguousJokerResolve(selectedWord) {
        this.sendWriteWordRequest(`${this.state.ambiguousJokerUrl}&spec=${selectedWord}`);
        this.setState({
            ambiguousJokerUrl: null,
            ambiguousJokerOptions: []
        });
    }

    handleAmbiguousJokerResolveCancel() {
        this.setState({
            ambiguousJokerUrl: null,
            ambiguousJokerOptions: []
        });
    }

    render() {
        let before = null;

        if (this.state.ambiguousJokerUrl != null) {
            before = (
                <div className="overlay fade-in">
                    <AmbiguousJokerResolvePopup
                        words={this.state.ambiguousJokerOptions}
                        onWordClick={this.handleAmbiguousJokerResolve}
                        onCancel={this.handleAmbiguousJokerResolveCancel} />
                </div>
            );
        }

        return (
            <div className="game-container">
                {before}
                <MessageBoard id="default" />
                <Leaderboard />
                <GameBoard game={this.state.game} ref={this.board}
                    zoomEnabled={before == null}
                    onLetterPlop={this.handleLetterPlop}
                    onWordCancel={this.handleWordCancel} />
                <GameMenu game={this.state.game}
                    onLetterSlideIn={this.handleLetterSlideIn}
                    onLetterDrag={this.handleLetterDrag}
                    onLetterDrop={this.handleLetterDrop}
                    onDraggedLetterSet={this.handleDraggedLetterSet}
                    onWordConfirm={this.handleWordConfirm}
                    onWordCancel={this.handleWordCancel} />
            </div>
        );
    }
}

GameContainer.defaultProps = {
    visible: true
}