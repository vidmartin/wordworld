
import { GameMenu } from "./GameMenu.jsx";
import { GameBoard } from "./GameBoard.jsx";

export class GameContainer extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            game: GAME_INITIAL
        };

        this.cache = {
            fetchingStatusRequest: null
        };

        this.board = React.createRef();

        this.fetchStatus = this.fetchStatus.bind(this);
        this.handleLetterSlideIn = this.handleLetterSlideIn.bind(this);
        this.handleLetterDrag = this.handleLetterDrag.bind(this);
        this.handleLetterDrop = this.handleLetterDrop.bind(this);
        this.handleDraggedLetterSet = this.handleDraggedLetterSet.bind(this);
    }

    componentDidMount() {
        // this.fetchStatus(); 
    }

    // fetch player's score & inventory and update state accordingly
    // (získat info o hráčově skóre & inventáři a updatovat podle toho stav)
    // newLetters is the amount of letters that were replaced - this is used to animate the newly drawn letters
    // (newLetters je počet písmenek, která byla vyměněna - to je použito pro animaci nově vylosovaných písmen)
    // ((český překlad uveden v zájmu jednoznačnosti, anžto po anglicku není jasné jestli 'drawn' znamená 'nakreslený' nebo 'vylosovaný'))
    fetchStatus() {
        let oldLetters = this.state.game.inventory.length - this.state.game.usedLetters.length; // how many letters will stay

        if (this.cache.fetchingStatusRequest != null) {
            // we don't wanna be fetching status multiple times at once
            console.log("aborting ongoing player status request");
            this.cache.fetchingStatusRequest.abort();
            console.log("aborted ongoing player status request");
        }

        // TODO: do this without jQuery?
        this.cache.fetchingStatusRequest = $.getJSON(`/game/status?token=${PLAYER_TOKEN}`, data => {
            if (data.status == "ok") {
                let biggestCurrentKey = this.state.game.inventory.reduce((prev, curr) => Math.max(prev, curr.key), 0);

                let letterArray = Array.from(data.data.inventory); // array from string
                let minNewIndex = oldLetters - 1;
                letterArray = letterArray
                    .filter((_, i) => i >= minNewIndex)
                    .map((ch, i) => ({ key: biggestCurrentKey + i + 1, letter: ch }));

                this.setState({
                    game: { ...this.state.game,
                        score: data.data.score,
                        inventory: this.state.game.inventory
                            .filter(item => this.state.game.usedLetterKeys.indexOf(item.key) == -1)
                            .concat(letterArray),
                        lettersToAnimate: letterArray.length,
                        usedLetterKeys: []
                    }
                });

                this.cache.unfetchedNewLetters = 0;
            }
        }).fail(() => {
            console.log("status fetch failed");
        }).always(() => {
            console.log("status fetch completed, setting fetchingStatusRequest to null");
            this.cache.fetchingStatusRequest = null;
        });
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
        // TODO: this is where the code to drop letters onto the board will be 

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

    render() {
        return (
            <div className="game-container">
                <GameBoard game={this.state.game} ref={this.board}
                    onLetterPlop={this.handleLetterPlop} />
                <GameMenu game={this.state.game}
                    onLetterSlideIn={this.handleLetterSlideIn}
                    onLetterDrag={this.handleLetterDrag}
                    onLetterDrop={this.handleLetterDrop}
                    onDraggedLetterSet={this.handleDraggedLetterSet} />
            </div>
        );
    }
}

GameContainer.defaultProps = {
    visible: true
}