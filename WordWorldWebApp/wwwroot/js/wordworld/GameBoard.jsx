
import { cssVar } from "./cssvar.js";
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
var PLACED_VALID_CELL_COLOR = cssVar("--colo-placed-letter-valid").get();
var CELL_FONT_COLOR = cssVar("--color-white").get();
var BOARD_RECT_PADDING = 10;

// TODO: dodělat

export class GameBoard extends React.Component {
    constructor(props) {
        super(props);

        this.canvas = React.createRef();
        
        this.state = {
            boardRect: { x: 0, y: 0, w: 0, h: 0 },
            boardArray: "",
            origin: props.game.origin,
            zoom: 1,
            placedLetters: {}
        };

        this.cache = {
            fetchingBoardRequest: null,
            isDragging: false,
            dragStartMousePos: { x: 0, y: 0 },
            dragStartOrigin: props.game.origin,
            highlightedSquare: null
        };

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
                    onMouseDown={this.handleCanvasMouseDown} />
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
            context.scale(cellSize.x, cellSize.y);
            context.translate(-this.state.origin.x, -this.state.origin.y);

            context.textAlign = "center";
            context.textBaseline = "middle";
            context.font = "0.3px 'Roboto Slab'";
            
            for (let x = 0; x < this.state.boardRect.w; x++) {
                for (let y = 0; y < this.state.boardRect.h; y++) {
                    let letterChar = this.state.boardArray[y * this.state.boardRect.w + x];

                    // coordinates relative to entire board (x & y vars are relative to current section)
                    let actualBoardX = x + this.state.boardRect.x; 
                    let actualBoardY = y + this.state.boardRect.y;

                    let placedLetter = this.state.placedLetters[`${actualBoardX},${actualBoardY}`];                   

                    if (placedLetter == undefined) {
                        context.fillStyle = this.getCellFillColor({ x: actualBoardX, y: actualBoardY }, letterChar);
                    } else {
                        context.fillStyle = PLACED_INVALID_CELL_COLOR;
                        letterChar = placedLetter.letter;
                    }   
                    
                    context.fillRect(actualBoardX - CELL_RADIUS, actualBoardY - CELL_RADIUS, CELL_RADIUS * 2, CELL_RADIUS * 2);

                    context.fillStyle = CELL_FONT_COLOR;
                    context.fillText(letterChar.toUpperCase(), actualBoardX, actualBoardY);
                }
            }
        } finally {
            context.restore();
        }
    }

    getCellFillColor(position, letterChar) {
        if (this.cache.highlightedSquare != null
            && position.x == this.cache.highlightedSquare.x
            && position.y == this.cache.highlightedSquare.y) {
            return HIGHLIGHT_EMPTY_CELL_COLOR;
        } else {
            return (letterChar == " " || letterChar == "\0") ? EMPTY_CELL_COLOR : FILLED_CELL_COLOR;
        }
    }

    componentDidMount() {
        this.draw();
        
        setInterval(this.fetchBoard, 500);
    }

    componentDidUpdate() {
        this.draw();
    }  

    fetchBoard() {
        if (this.cache.fetchingBoardRequest != null) {
            return;
        }

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

        this.cache.fetchingBoardRequest = $.getJSON(`/game/scan?token=${PLAYER_TOKEN}&x=${x}&y=${y}&w=${w}&h=${h}`, data => {
            if (data.status == "ok") {                
                this.setState({
                    boardRect: { x: x, y: y, w: w, h: h },
                    boardArray: data.data.board
                });                
            }
        }).always(() => {
            this.cache.fetchingBoardRequest = null;
        })
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

            this.cache.highlightedSquare = { x: Math.round(x), y: Math.round(y) };

            this.draw();
        }
    }

    handleCanvasMouseDown() {
        // code to start dragging
        this.cache.dragStartMousePos = getMousePos();
        this.cache.dragStartOrigin = this.state.origin;
        this.cache.isDragging = true;
    }

    handleMouseUp() {
        if (this.cache.isDragging) {
            // code to stop dragging
            this.cache.isDragging = false;            
        }
    }

    handleWheel(ev) {
        this.setState({
            zoom: Math.min(MAX_ZOOM, Math.max(MIN_ZOOM, this.state.zoom + ev.deltaY * ZOOM_MULTIPLIER))
        });
    }

    getCellSize() {
        return {
            x: CELL_SIZE_PX.x * Math.pow(ZOOM_BASE, this.state.zoom),
            y: CELL_SIZE_PX.y * Math.pow(ZOOM_BASE, this.state.zoom)
        };
    }

    dropLetter() {
        if (this.isMouseOverCanvas() == false) {
            return false;
        }

        if (this.getCharAtGlobal(this.cache.highlightedSquare) == " ") {
            this.setState({
                placedLetters: {
                    ...this.state.placedLetters,
                    [`${this.cache.highlightedSquare.x},${this.cache.highlightedSquare.y}`]: this.props.game.currentlyDraggedLetter 
                }
            });

            return true;
        }

        return false;
    }

    getCharAtGlobal(position) {
        let placedLetter = this.state.placedLetters[`${position.x},${position.y}`];
        if (placedLetter != undefined) {
            return placedLetter.letter;
        }

        let x = position.x - this.state.boardRect.x;
        let y = position.y - this.state.boardRect.y;
        let i = y * this.state.boardRect.w + x;

        if (i < 0 || i >= this.state.boardArray.length) {
            return null;
        }

        return this.state.boardArray[i];
    }
}