// ============================================
// CampusPulse+ AUTH ANIMATION ENGINE
// ============================================

const scene = document.querySelector(".auth-scene");

const board = document.getElementById("noticeBoard");

const boardTitle = document.getElementById("boardTitle");

const boardMessage = document.getElementById("boardMessage");

const studentLeft = document.getElementById("studentLeft");

const studentRight = document.getElementById("studentRight");

const authCard = document.querySelector(".account-box");

const dustLayer = document.querySelector(".dust-layer");

const impact = document.querySelector(".push-impact");

let animationRunning = false;
// ============================================
// PAGE TYPE
// ============================================

const pageName = document.body.dataset.page || "login";

const messages = {

    login: {
        title: "Welcome Back 👋",
        text: "Please login to continue."
    },

    register: {
        title: "New Student?",
        text: "Please register first!"
    },

    forgot: {
        title: "Forgot Password?",
        text: "Let's recover your account."
    },

    verify: {
        title: "Verify Email",
        text: "Verification required."
    }

};// ============================================
// UTILITY FUNCTIONS
// ============================================

function sleep(ms) {

    return new Promise(resolve => setTimeout(resolve, ms));

}

function updateBoard(title, message) {

    if (!board) return;

    boardTitle.innerHTML = title;

    boardMessage.innerHTML = message;

}

function showBoard() {

    if (!board) return;

    board.classList.add("show");

}

function hideBoard() {

    if (!board) return;

    board.classList.remove("show");

}

function showStudents() {

    studentLeft.classList.add("enter");

    studentRight.classList.add("enter");

}

function hideStudents() {

    studentLeft.classList.remove("enter");

    studentRight.classList.remove("enter");

}
// ============================================
// WALKING CONTROLLER
// ============================================

async function walkIn(title, message) {

    updateBoard(title, message);

    showBoard();

    showStudents();

    studentLeft.classList.add("walking");

    studentRight.classList.add("walking");

    await sleep(2600);

    studentLeft.classList.remove("walking");

    studentRight.classList.remove("walking");

}

async function waveStudents() {

    studentLeft.classList.add("wave");

    studentRight.classList.add("wave");

    await sleep(1200);

    studentLeft.classList.remove("wave");

    studentRight.classList.remove("wave");

}

async function exitStudents() {

    studentLeft.classList.add("exit");

    studentRight.classList.add("exit");

    await sleep(2200);

    studentLeft.classList.remove("exit");

    studentRight.classList.remove("exit");

    hideBoard();

    hideStudents();

}
// ============================================
// DUST EFFECT
// ============================================

function playDust() {

    if (!dustLayer) return;

    dustLayer.classList.remove("active");

    void dustLayer.offsetWidth;

    dustLayer.classList.add("active");

}

// ============================================
// IMPACT EFFECT
// ============================================

function playImpact() {

    if (!impact) return;

    impact.classList.remove("active");

    void impact.offsetWidth;

    impact.classList.add("active");

}

// ============================================
// CARD PUSH
// ============================================

async function pushCard() {

    if (!authCard) return;

    authCard.classList.add("shake");

    await sleep(250);

    authCard.classList.remove("shake");

    studentLeft.classList.add("push");

    studentRight.classList.add("push");

    playDust();

    playImpact();

    authCard.classList.add("push-left");

    await sleep(900);

    studentLeft.classList.remove("push");

    studentRight.classList.remove("push");

}

// ============================================
// COMPLETE TRANSITION
// ============================================

async function playTransition(url, title, message) {

    if (animationRunning)
        return;

    animationRunning = true;

    await walkIn(title, message);

    await pushCard();

    await waveStudents();

    await exitStudents();

    document.body.classList.add("page-fade");

    await sleep(500);

    window.location.href = url;

}
// ============================================
// NAVIGATION INTERCEPTOR
// ============================================

document.addEventListener("DOMContentLoaded", () => {

    // -----------------------------
    // REGISTER
    // -----------------------------

    document.querySelectorAll(
        'a[href*="/Account/Register"]'
    ).forEach(link => {

        link.addEventListener("click", async function (e) {

            e.preventDefault();

            await playTransition(

                this.href,

                "🎓 New Student?",

                "Please Register First!"

            );

        });

    });

    // -----------------------------
    // LOGIN
    // -----------------------------

    document.querySelectorAll(
        'a[href*="/Account/Login"]'
    ).forEach(link => {

        link.addEventListener("click", async function (e) {

            e.preventDefault();

            await playTransition(

                this.href,

                "👋 Welcome Back",

                "Please Login"

            );

        });

    });

    // -----------------------------
    // VERIFY EMAIL
    // -----------------------------

    document.querySelectorAll(
        'a[href*="/Account/VerifyEmail"]'
    ).forEach(link => {

        link.addEventListener("click", async function (e) {

            e.preventDefault();

            await playTransition(

                this.href,

                "🔑 Forgot Password?",

                "Let's recover your account."

            );

        });

    });

});
