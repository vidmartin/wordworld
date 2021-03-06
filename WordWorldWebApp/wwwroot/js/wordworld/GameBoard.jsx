
import { cssVar } from "./cssvar.js";
import { Letter } from "./Letter.jsx";
import { MessageBoard } from "./MessageBoard.jsx";
import { getMousePos } from "./mouse.js";

// props: board array, board section top + left + width + height
// state: position on board 

var CELL_SIZE_PX = { x: 60, y: 60 };
var CELL_RADIUS = 0.4;
var ZOOM_BASE = 1.1;
var ZOOM_MULTIPLIER = -0.01;
var MAX_ZOOM = 10;
var MIN_ZOOM = -10;

var EMPTY_CELL_COLOR = cssVar("--color-empty-cell").get();
var HIGHLIGHT_EMPTY_CELL_COLOR = cssVar("--color-highlight-empty-cell").get();
var FILLED_CELL_COLOR = cssVar("--color-filled-cell").get();
var PLACED_INVALID_CELL_COLOR = cssVar("--color-placed-letter-invalid").get();
var PLACED_VALID_CELL_COLOR = cssVar("--color-placed-letter-valid").get();
var CELL_FONT_COLOR = cssVar("--color-white").get();
var BOARD_RECT_PADDING = 10;
var CANVAS_RESOLUTION = 10; // firefox měl problém s velikostí fontu (velký scale, mini font), tak je třeba to ošulit

export class GameBoard extends React.Component {
    constructor(props) {
        super(props);

        this.canvas = React.createRef();
        
        this.state = {
            boardRect: { x: 0, y: 0, w: 0, h: 0 },
            boardArray: "",
            origin: props.game.origin,
            zoom: 1,
            placedLetters: {},
            wordPos: null,
            wordStr: null,
            wordDir: null
        };

        this.wordStack = [];

        this.cache = {
            isFetchingBoard: false,
            isDragging: false,
            dragStartMousePos: { x: 0, y: 0 },
            dragStartOrigin: props.game.origin,
            highlightedSquare: null
        };

        // javascript aneb fujky fujky
        this.isMouseOverCanvas = this.isMouseOverCanvas.bind(this);
        this.draw = this.draw.bind(this);
        this.fetchBoard = this.fetchBoard.bind(this);
        this.recalculateCanvasArea = this.recalculateCanvasArea.bind(this);
        this.handleMouseMove = this.handleMouseMove.bind(this);
        this.handleCanvasMouseDown = this.handleCanvasMouseDown.bind(this);
        this.handleMouseUp = this.handleMouseUp.bind(this);
        this.getCellFillColor = this.getCellFillColor.bind(this);
        this.handleWheel = this.handleWheel.bind(this);
        this.getCellSize = this.getCellSize.bind(this);
        this.dropLetter = this.dropLetter.bind(this);
        this._dropLetter = this._dropLetter.bind(this);
        this.extendWord = this.extendWord.bind(this);
        this.getWord = this.getWord.bind(this);
        this.setWord = this.setWord.bind(this);
        this.clearWord = this.clearWord.bind(this);
        this.checkWordOverlap = this.checkWordOverlap.bind(this);
        this.fetchBoardAndCheckOverlap = this.fetchBoardAndCheckOverlap.bind(this);

        window.addEventListener("resize", this.draw);
        window.addEventListener("mousemove", this.handleMouseMove);
        window.addEventListener("mouseup", this.handleMouseUp);
        window.addEventListener("wheel", this.handleWheel);
    }

    isMouseOverCanvas() {
        let mousePos = getMousePos();
        let boundingRect = this.canvas.current.getBoundingClientRect();

        if (mousePos.x > boundingRect.x && mousePos.y > boundingRect.y) {
            return mousePos.x < boundingRect.x + boundingRect.width && mousePos.y < boundingRect.y + boundingRect.height;
        }

        return false;
    }

    render() {
        return (
            <div className="game-board">
                <canvas ref={this.canvas}
                    onMouseDown={this.handleCanvasMouseDown}
                    onContextMenu={ev => ev.preventDefault()} />
            </div>
        );
    }

    draw() {
        this.recalculateCanvasArea();

        if (this.props.game.currentlyDraggedLetter == null) {
            this.cache.highlightedSquare = null; // if not dragging a letter, set this to null, to ensure that no square will be highlighted
        }

        let context = this.canvas.current.getContext("2d");

        context.save();

        try {
            let canvasW = this.canvas.current.width;
            let canvasH = this.canvas.current.height;

            context.clearRect(0, 0, canvasW, canvasH);
            // context.fillStyle = "#FFFFFF";
            // context.fillRect(0, 0, this.canvas.current.clientWidth, this.canvas.current.clientHeight);

            let cellSize = this.getCellSize();

            context.translate(Math.floor(canvasW / 2), Math.floor(canvasH / 2));
            context.scale(cellSize.x / CANVAS_RESOLUTION, cellSize.y / CANVAS_RESOLUTION);
            context.translate(-this.state.origin.x * CANVAS_RESOLUTION, -this.state.origin.y * CANVAS_RESOLUTION);

            context.textAlign = "center";
            context.textBaseline = "middle";
            //context.font = "0.3px 'Roboto Slab'";
            context.font = "3px 'Roboto Slab'";
            
            for (let x = 0; x < this.state.boardRect.w; x++) {
                for (let y = 0; y < this.state.boardRect.h; y++) {
                    // coordinates relative to entire board (x & y vars are relative to current section)
                    let actualPos = { x: x + this.state.boardRect.x, y: y + this.state.boardRect.y };

                    let letterChar = this.getCharAtGlobal(actualPos, false);

                    if (letterChar == null) {
                        continue;
                    }

                    context.fillStyle = this.getCellFillColor(actualPos, letterChar);
                    
                    if (letterChar == " " || letterChar == "\0") {
                        letterChar = this.getCharAtGlobal(actualPos, true);                        
                    }

                    context.fillRect(
                        (actualPos.x - CELL_RADIUS) * CANVAS_RESOLUTION,
                        (actualPos.y - CELL_RADIUS) * CANVAS_RESOLUTION,
                        CELL_RADIUS * 2 * CANVAS_RESOLUTION,
                        CELL_RADIUS * 2 * CANVAS_RESOLUTION);

                    context.fillStyle = CELL_FONT_COLOR;
                    
                    context.fillText(letterChar.toUpperCase(), actualPos.x * CANVAS_RESOLUTION, actualPos.y * CANVAS_RESOLUTION);
                }
            }
        } finally {
            context.restore();
        }
    }

    getCellFillColor(position, letterChar) {
        if (letterChar == " " || letterChar == "\0") {
            if (this.getIndexInWord(position) >= 0) {
                return PLACED_INVALID_CELL_COLOR;
            }

            if (this.cache.highlightedSquare != null
                && position.x == this.cache.highlightedSquare.x
                && position.y == this.cache.highlightedSquare.y) {
                return HIGHLIGHT_EMPTY_CELL_COLOR;
            } else {
                return EMPTY_CELL_COLOR;
            }
        }

        return FILLED_CELL_COLOR;
    }

    componentDidMount() {
        this.fetchBoardAndCheckOverlap();
        //this.draw();

        setInterval(this.fetchBoardAndCheckOverlap, 500); // regularly download board information
    }

    componentDidUpdate() {
        this.draw();
    }  

    fetchBoardAndCheckOverlap() {
        this.fetchBoard();

        if (this.checkWordOverlap()) {
            MessageBoard.writeError("Someone used the tiles before us!");
            this.props.onWordCancel();
        }

        this.setState(this.state);
    }

    async fetchBoard() {
        if (this.cache.isFetchingBoard == true) {
            // we don't want multiple requests at once
            return;
        }

        this.cache.isFetchingBoard = true;

        try {
            let cellSize = this.getCellSize();

            let x = Math.floor(this.state.origin.x - this.canvas.current.width / 2 / cellSize.x - BOARD_RECT_PADDING);
            let y = Math.floor(this.state.origin.y - this.canvas.current.height / 2 / cellSize.y - BOARD_RECT_PADDING);

            x = Math.max(0, x);
            y = Math.max(0, y);

            let x2 = Math.floor(this.state.origin.x + this.canvas.current.width / 2 / cellSize.x + BOARD_RECT_PADDING);
            let y2 = Math.floor(this.state.origin.y + this.canvas.current.height / 2 / cellSize.y + BOARD_RECT_PADDING);

            x2 = Math.min(BOARD_SIZE.x, x2);
            y2 = Math.min(BOARD_SIZE.y, y2);

            let w = x2 - x + 1;
            let h = y2 - y + 1;

            let response = await fetch(`/game/scan?token=${PLAYER_TOKEN}&x=${x}&y=${y}&w=${w}&h=${h}`);

            if (response.ok != true) {
                console.error(`board fetch failed - http error '${response.statusText}'`);
                return;
            }

            let json = await response.json();

            if (json.status == "ok") {
                this.setState({
                    boardRect: { x: x, y: y, w: w, h: h },
                    boardArray: json.data.board
                });
            } else if (json.status == "player_not_found") {
                // if the player was not found (likely removed due to inactivity) we redirect to homepage
                // TODO: handle this somewhere else (it's not really a concern of the game board)
                window.location.href = "/";
            }
        } finally {
            this.cache.isFetchingBoard = false;
        }
    }

    recalculateCanvasArea() {
        this.canvas.current.width = this.canvas.current.clientWidth;
        this.canvas.current.height = this.canvas.current.clientHeight;
    }

    handleMouseMove(ev) {
        let cellSize = this.getCellSize();

        if (this.cache.isDragging) {
            // dragging board => move origin

            let mouseDelta = {
                x: ev.clientX - this.cache.dragStartMousePos.x,
                y: ev.clientY - this.cache.dragStartMousePos.y
            };

            this.setState({
                origin: {
                    x: this.cache.dragStartOrigin.x - mouseDelta.x / cellSize.x,
                    y: this.cache.dragStartOrigin.y - mouseDelta.y / cellSize.y
                }
            });
        } else if (this.props.game.currentlyDraggedLetter != null) {
            // dragging letter => highlight square

            let x = this.state.origin.x + (ev.clientX - this.canvas.current.clientWidth / 2) / cellSize.x;
            let y = this.state.origin.y + (ev.clientY - this.canvas.current.clientHeight / 2) / cellSize.y;

            // TODO: don't highlight, if placement would be invalid

            this.cache.highlightedSquare = { x: Math.round(x), y: Math.round(y) };

            this.draw();
        }
    }

    handleCanvasMouseDown(ev) {
        if (ev.button == 0) {
            // code to start dragging
            this.cache.dragStartMousePos = getMousePos();
            this.cache.dragStartOrigin = this.state.origin;
            this.cache.isDragging = true;
        }
    }

    handleMouseUp(ev) {
        if (this.cache.isDragging && ev.button == 0) {
            // code to stop dragging
            this.cache.isDragging = false;            
        }
    }

    handleWheel(ev) {
        if (this.props.zoomEnabled == true) {
            this.setState({
                zoom: Math.min(MAX_ZOOM, Math.max(MIN_ZOOM, this.state.zoom + ev.deltaY * ZOOM_MULTIPLIER))
            });
        }
    }

    getCellSize() {
        return {
            x: CELL_SIZE_PX.x * Math.pow(ZOOM_BASE, this.state.zoom),
            y: CELL_SIZE_PX.y * Math.pow(ZOOM_BASE, this.state.zoom)
        };
    }

    getWord() {
        if (this.state.wordPos == null || this.state.wordStr == null) {
            return null;
        }

        return {
            wordDir: this.state.wordDir,
            wordPos: this.state.wordPos,
            wordStr: this.state.wordStr
        };
    }

    setWord(wordObject, update = false) {
        this.state.wordDir = wordObject.wordDir;
        this.state.wordPos = wordObject.wordPos;
        this.state.wordStr = wordObject.wordStr;
        if (update) {
            this.forceUpdate();
        }
    }

    _dropLetter() {
        if (this.getCharAtGlobal(this.cache.highlightedSquare) == " ") {

            if (this.state.wordPos == null) {
                // first letter placed - new word started

                this.setState({
                    wordPos: { ...this.cache.highlightedSquare },
                    wordStr: this.props.game.currentlyDraggedLetter.letter
                });

                return true;

            } else if (this.state.wordDir == null) {
                // second letter placed - direction set

                let rememberedState = this.getWord() // remember current state of word in case this fails

                if (this.cache.highlightedSquare.x == this.state.wordPos.x) {
                    this.state.wordDir = "y"; // vertical dir
                } else if (this.cache.highlightedSquare.y == this.state.wordPos.y) {
                    this.state.wordDir = "x"; // horizontal dir
                } else {
                    return false;
                }

                this.extendWord(); // include adjacent letters in word

                // try to put the new letter - call this again, but now direction is specified
                if (this._dropLetter() == true) {
                    return true;
                } else {
                    this.setWord(rememberedState, true); // restore the remembered state of word
                    return false;
                }

            } else {
                // adding more letters

                let theVariableThatDeterminesWhetherWeArePuttingALetterToTheFrontOfTheWordOrToTheEnd = null;

                if (this.cache.highlightedSquare.x == this.state.wordPos.x && this.state.wordDir == "y") {
                    theVariableThatDeterminesWhetherWeArePuttingALetterToTheFrontOfTheWordOrToTheEnd = this.cache.highlightedSquare.y - this.state.wordPos.y;
                } else if (this.cache.highlightedSquare.y == this.state.wordPos.y && this.state.wordDir == "x") {
                    theVariableThatDeterminesWhetherWeArePuttingALetterToTheFrontOfTheWordOrToTheEnd = this.cache.highlightedSquare.x - this.state.wordPos.x;
                } else {
                    return false;
                }

                if (theVariableThatDeterminesWhetherWeArePuttingALetterToTheFrontOfTheWordOrToTheEnd > 0) {
                    theVariableThatDeterminesWhetherWeArePuttingALetterToTheFrontOfTheWordOrToTheEnd -= this.state.wordStr.length;
                    theVariableThatDeterminesWhetherWeArePuttingALetterToTheFrontOfTheWordOrToTheEnd += 1;
                }

                // DONE: allow letters that are already on the board to be part of the new word

                switch (theVariableThatDeterminesWhetherWeArePuttingALetterToTheFrontOfTheWordOrToTheEnd) {
                    case 1:
                        this.setState({
                            wordStr: this.state.wordStr + this.props.game.currentlyDraggedLetter.letter
                        });
                        return true;

                    case -1:
                        this.setState({
                            wordPos: this.state.wordDir == "y" ? { x: this.state.wordPos.x, y: this.state.wordPos.y - 1 } : { x: this.state.wordPos.x - 1, y: this.state.wordPos.y },
                            wordStr: this.props.game.currentlyDraggedLetter.letter + this.state.wordStr
                        });
                        return true;

                    default:
                        return false;
                }
            }

            throw "this code should not have been reached";
        }

        return false;
    }

    dropLetter() {
        if (this.isMouseOverCanvas() == false) {
            return false;
        }

        if (this._dropLetter()) {
            this.extendWord();
            return true;
        }

        return false;
    }

    // there are characters on the board immediately after or before the word we are building, add these characters to the word
    extendWord(dir = null) {        
        let step = null;

        switch (dir ?? this.state.wordDir) {
            case "y":
                step = { x: 0, y: 1 };
                break;
            case "x":
                step = { x: 1, y: 0 };
                break;
            case null:
                return;
            default:
                throw "peculiar things are happening";
        }

        let stepFunc = (pos, i) => ({ x: pos.x + step.x * i, y: pos.y + step.y * i });

        let endPos = stepFunc(this.state.wordPos, this.state.wordStr.length);        

        while (true) {
            // while there are characters at the end of the word, at them to it

            let char = this.getCharAtGlobal(endPos, false);

            if (char == " " || char == null) {
                break;
            }

            this.state.wordStr += char;
            endPos = stepFunc(endPos, 1);
        }

        let startPos = stepFunc(this.state.wordPos, -1);

        while (true) {
            // while there are characters at the start of the word, add them to it

            let char = this.getCharAtGlobal(startPos, false);

            if (char == " " || char == null) {
                break;
            }

            this.state.wordStr = char + this.state.wordStr;
            this.state.wordPos = startPos;
            startPos = stepFunc(startPos, -1);
        }
    }

    getIndexInWord(position) {
        // convert the global board position into an index in the newly created word. if the position is not in the word or the word hasn't been started yet, returns -1.

        if (this.state.wordPos == null || this.state.wordStr == null) {
            return -1;
        }

        let i = null;

        if (position.x == this.state.wordPos.x && position.y == this.state.wordPos.y) {
            return 0;
        } else if (this.state.wordDir == "y" && position.x == this.state.wordPos.x) {
            i = position.y - this.state.wordPos.y;
        } else if (this.state.wordDir == "x" && position.y == this.state.wordPos.y) {
            i = position.x - this.state.wordPos.x;
        } else {
            return -1;
        }

        if (i < 0 || i >= this.state.wordStr.length) {
            return -1;
        } else {
            return i;
        }
    }

    getCharAtGlobal(position, includeOurWord = true) {  

        if (includeOurWord == true) {
            let wi = this.getIndexInWord(position);
            if (wi >= 0) {
                return this.state.wordStr[wi];
            }
        }

        let x = position.x - this.state.boardRect.x;
        let y = position.y - this.state.boardRect.y;
        let i = y * this.state.boardRect.w + x;

        if (i < 0 || i >= this.state.boardArray.length) {
            return null;
        }

        return this.state.boardArray[i];
    }

    clearWord() {
        this.setState({
            wordDir: null,
            wordPos: null,
            wordStr: null
        });
    }

    getWordSafeDir() {
        // in case we are trying to finish the word with only one letter, direction isn't decided yet
        // if this.state.wordDir is null, this method will return a word with the direction, in which a longer word is formed

        let rememberedWord = this.getWord();

        if (rememberedWord == null) {
            return null;
        }

        if (rememberedWord.wordDir != null) {
            // if direction is filled in, we have no reason to infer it
            return rememberedWord;
        }

        try {
            // if direction isn't specified yet, we decide it based on in which direction a longer word is formed
            this.state.wordDir = "y";
            this.extendWord();
            let yWord = this.getWord();

            this.setWord(rememberedWord, false);

            this.state.wordDir = "x";
            this.extendWord();
            let xWord = this.getWord();

            return (yWord.wordStr.length > xWord.wordStr.length) ? yWord : xWord;

        } finally {
            this.setWord(rememberedWord, false); // restore original state
        }        
    }

    checkWordOverlap() {
        // check, whether the word we're building is overlapping with characters already on the board

        if (this.state.wordPos == null || this.state.wordStr == null) {
            return false;
        }

        let step = null;

        switch (this.state.wordDir) {
            case "y":
                step = { x: 0, y: 1 };
                break;
            case "x":
                step = { x: 1, y: 0 };
                break;
            case null:
                step = { x: 0, y: 0 };
                break;
            default:
                throw "peculiar things are happening";
        }

        let stepFunc = (pos, i) => ({ x: pos.x + step.x * i, y: pos.y + step.y * i });

        for (let i = 0; i < this.state.wordStr.length; i++) {
            let pos = stepFunc(this.state.wordPos, i);

            let ch = this.getCharAtGlobal(pos, false);
            if (ch != " " && ch != "\0" && ch != this.state.wordStr[i]) {
                return true; // overlap found
            }
        }

        return false;
    }
}