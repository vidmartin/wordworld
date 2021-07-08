
function startGame() {
    let username = document.getElementsByName("username")[0].value;
    let board = document.getElementsByName("board")[0].value;

    window.location.href = `/play/${board}?username=${username}`;
}