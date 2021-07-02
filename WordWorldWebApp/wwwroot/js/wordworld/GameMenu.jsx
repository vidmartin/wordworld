
import { Letter } from "./Letter.jsx";
import { getMousePos } from "./mouse.js";

export class GameMenu extends React.Component {
    constructor(props) {
        super(props);

        //this.state = {
        //    currentlyDraggedLetterKey: null
        //};        

        this.cache = {
            letterOffset: { x: 0, y: 0 },
            letterSlideInReady: true
        };

        this.handleMouseMove = this.handleMouseMove.bind(this);
        this.handleMouseUp = this.handleMouseUp.bind(this);
        this.handleLetterMouseDown = this.handleLetterMouseDown.bind(this);

        window.addEventListener("mousemove", this.handleMouseMove);
        window.addEventListener("mouseup", this.handleMouseUp);
    }

    handleMouseMove(e) {
        if (this.props.game.currentlyDraggedLetter != null) {
            this.props.onLetterDrag?.(getMousePos());
            this.forceUpdate(); // force update to move the dragged letter; TODO: maybe move the DOM element of the letter imperatively for performance?           
        }
    }

    handleMouseUp(e) {
        if (this.props.game.currentlyDraggedLetter != null) {
            this.props.onLetterDrop(getMousePos());
            this.props.onDraggedLetterSet(null);            
        }
    }

    handleLetterMouseDown(e, letterComponent) {
        if (e.button == 0) {

            if (this.props.game.currentlyDraggedLetter != null) {
                return; // we don't wanna be draggin' two letters at once
            }

            this.props.onDraggedLetterSet?.(letterComponent.props.letter.key);

            // remember offset to top left corner of dragged element
            let clientRect = letterComponent.div.current.getBoundingClientRect();
            this.cache.letterOffset = {
                x: getMousePos().x - clientRect.x,
                y: getMousePos().y - clientRect.y
            };

            this.forceUpdate();
        }
    }    

    render() {
        let letters = this.props.game.inventory;
        let firstNewIndex = letters.length - this.props.game.lettersToAnimate;
        // console.log(`letters to animate: ${this.props.game.lettersToAnimate}`);

        let letterItems = letters.map((letter, i) => {
            if (this.props.game.currentlyDraggedLetter == letter) {
                    
                // dragged letters shall be rendered differently
                return (
                    <Letter key={letter.key} index={i} dragging={true}
                        letter={letter}
                        posX={getMousePos().x - this.cache.letterOffset.x}
                        posY={getMousePos().y - this.cache.letterOffset.y} />                        
                );
            }

            let hidden = i >= firstNewIndex; // letters from a certain index are hidden on the right and will slide in later
            let visible = this.props.game.usedLetterKeys.indexOf(letter.key) == -1; // letters that were placed on the board will not be visible in menu

            return (
                <Letter key={letter.key} index={i} dragging={false}
                    letter={letter}
                    hiddenLeft="25cm" hidden={hidden}
                    visible={visible}
                    onMouseDown={this.handleLetterMouseDown} />
            );
        });

        if (this.props.game.lettersToAnimate > 0 && this.cache.letterSlideInReady) {
            // if there are letters left to animate, this will trigger onLetterSlideIn callback after a delay
            // GameContainer will handle this callback by reducing the lettersToAnimate property by one,
            // triggering a re-render with one letter not being hidden anymore, which will cause a nice animation to happen thanks to css transitions
            // this vicious cycle will repeat until lettersToAnimate is zero
            this.cache.letterSlideInReady = false;
            setTimeout(() => {
                this.props.onLetterSlideIn?.();
                this.cache.letterSlideInReady = true; // re-enable this
            }, 200);            
        }

        return (
            <div className="game-menu">
                <div className="letter-container">
                    {letterItems}
                </div>
            </div>
        );
    }
}