<!DOCTYPE html>
<html lang="it">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>Zoro Game</title>
<style>
  body { margin: 0; background: black; }
  canvas { display: block; margin: auto; background: #000; }
</style>
</head>
<body>
<canvas id="gameCanvas" width="800" height="480"></canvas>

<script>
// ======== VARIABILI DI GIOCO ========
const canvas = document.getElementById("gameCanvas");
const ctx = canvas.getContext("2d");

let keys = {};
let gameOver = false;
let fireworksActive = false;
let fireworksTimer = 0;
let currentLevel = 0;
let score = 0;

const levels = [
  { length: 3000, enemyCount: 3, blockCount: 5 },
  { length: 4000, enemyCount: 5, blockCount: 7 },
  { length: 5000, enemyCount: 7, blockCount: 10 }
];

let player = {
  x: 400, y: 300, vy: 0,
  width: 40, height: 60,
  onGround: false,
  health: 100,
  canShoot: false,
  shootTimer: 0
};

let enemies = [];
let blocks = [];
let fireballs = [];

let cameraX = 0;

// ======== EVENTI ========
document.addEventListener("keydown", e => {
  keys[e.code] = true;
  if(e.code === "KeyZ" && player.canShoot){
    shootFireball();
  }
});
document.addEventListener("keyup", e => keys[e.code] = false);

// ======== FUNZIONI DI DISEGNO ========
function drawBackground(){
  ctx.fillStyle = "#222";
  ctx.fillRect(0,0,canvas.width,canvas.height);

  // Città stile Tekken
  for(let i=0;i<canvas.width;i+=100){
    ctx.fillStyle = "#444";
    ctx.fillRect(i - (cameraX % 100), canvas.height - 200, 80, 200);
  }

  // Insegna Zoro Game
  ctx.fillStyle = "red";
  ctx.font = "30px Arial";
  ctx.fillText("Zoro Game", 20, 40);
}

function drawPlayer(){
  ctx.fillStyle = "#FFD700";
  ctx.fillRect(player.x - cameraX, player.y, player.width, player.height);
  // Codino
  ctx.fillStyle = "#000";
  ctx.fillRect(player.x - cameraX + 30, player.y + 10, 8, 8);
  // Pizzetto
  ctx.fillRect(player.x - cameraX + 15, player.y + 50, 10, 5);
}

function drawEnemies(){
  ctx.fillStyle = "purple";
  enemies.forEach(e => {
    ctx.fillRect(e.x - cameraX, e.y, e.width, e.height);
  });
}

function drawBlocks(){
  ctx.fillStyle = "brown";
  blocks.forEach(b => {
    ctx.fillRect(b.x - cameraX, b.y, b.size, b.size);
  });
}

function drawFireballs(){
  ctx.fillStyle = "orange";
  fireballs.forEach(fb => {
    ctx.beginPath();
    ctx.arc(fb.x - cameraX, fb.y, 5, 0, Math.PI*2);
    ctx.fill();
  });
}

function drawHealthBar(){
  ctx.fillStyle = "red";
  ctx.fillRect(canvas.width - 120, 20, 100, 10);
  ctx.fillStyle = "lime";
  ctx.fillRect(canvas.width - 120, 20, player.health, 10);
  ctx.strokeStyle = "#fff";
  ctx.strokeRect(canvas.width - 120, 20, 100, 10);
}

function drawScore(){
  ctx.fillStyle = "#fff";
  ctx.font = "16px Arial";
  ctx.fillText("Score: "+score, canvas.width - 120, 50);
}

function drawFireworks(){
  if(!fireworksActive) return;
  ctx.fillStyle = "yellow";
  for(let i=0;i<10;i++){
    let x = Math.random()*canvas.width;
    let y = Math.random()*canvas.height/2;
    ctx.beginPath();
    ctx.arc(x,y,3,0,Math.PI*2);
    ctx.fill();
  }
}

function drawGameOverMessage(){
  if(Math.floor(Date.now()/500)%2===0){
    ctx.fillStyle = "white";
    ctx.font = "30px Arial";
    ctx.fillText("Hai perso, sei solo un rapperino", 100, canvas.height/2);
  }
}

// ======== LOGICA ========
function spawnLevel(lv){
  enemies = [];
  blocks = [];
  fireballs = [];
  cameraX = 0;
  player.x = 400;
  player.y = 300;
  player.vy = 0;
  player.health = 100;
  player.canShoot = false;

  let lvl = levels[lv];
  for(let i=0;i<lvl.enemyCount;i++){
    enemies.push({ x: 800 + i*200, y: 380, width: 40, height: 60 });
  }
  for(let i=0;i<lvl.blockCount;i++){
    blocks.push({ x: 500 + i*250, y: 250, size: 40, bonus: Math.random()<0.5?"heart":"flower", broken: false });
  }
}

function shootFireball(){
  fireballs.push({ x: player.x+player.width, y: player.y+player.height/2, vx: 5 });
}

function updatePlayer(){
  // Movimento orizzontale
  if(keys["ArrowRight"]) player.x += 4;
  if(keys["ArrowLeft"]) player.x -= 4;

  // Salto
  if(keys["Space"] && player.onGround){
    player.vy = -10;
    player.onGround = false;
  }

  // Gravità
  player.vy += 0.5;
  player.y += player.vy;

  // Terra
  if(player.y + player.height > canvas.height - 20){
    player.y = canvas.height - 20 - player.height;
    player.vy = 0;
    player.onGround = true;
  }

  // Sparo temporaneo
  if(player.canShoot){
    player.shootTimer--;
    if(player.shootTimer<=0) player.canShoot = false;
  }

  // Camera segue player
  cameraX = player.x - canvas.width/2;
}

function updateFireballs(){
  fireballs.forEach(fb => fb.x += fb.vx);
  fireballs = fireballs.filter(fb => fb.x - cameraX < canvas.width);
}

function checkCollisions(){
  // Blocchi
  blocks.forEach(b => {
    if(!b.broken &&
       player.x < b.x + b.size &&
       player.x + player.width > b.x &&
       player.y < b.y + b.size &&
       player.y + player.height > b.y){
         // Colpo da sotto
         if(player.vy < 0 && player.y > b.y){
           b.broken = true;
           if(b.bonus === "heart") player.health = Math.min(100, player.health + 20);
           if(b.bonus === "flower"){ player.canShoot = true; player.shootTimer = 60*30; }
         }
    }
  });

  // Nemici
  enemies = enemies.filter(e => {
    if(player.x < e.x + e.width &&
       player.x + player.width > e.x &&
       player.y < e.y + e.height &&
       player.y + player.height > e.y){
         // Salto sopra
         if(player.vy > 0){
           score += 100;
           player.vy = -8;
           return false;
         } else {
           player.health -= 1;
           if(player.health <= 0){
             gameOver = true;
             setTimeout(() => {
               gameOver = false;
               currentLevel = 0;
               spawnLevel(currentLevel);
             }, 5000);
           }
         }
    }
    return true;
  });
}

function checkLevelComplete(){
  if(player.x > levels[currentLevel].length){
    fireworksActive = true;
    fireworksTimer = 60*5;
    setTimeout(() => {
      fireworksActive = false;
      currentLevel++;
      if(currentLevel >= levels.length){
        currentLevel = 0;
      }
      spawnLevel(currentLevel);
    }, 5000);
  }
}

function updateFireworksState(){
  if(fireworksActive) fireworksTimer--;
}

function loop(){
  updatePlayer();
  updateFireballs();
  checkCollisions();
  checkLevelComplete();
  updateFireworksState();

  drawBackground();
  drawBlocks();
  drawEnemies();
  drawFireballs();
  drawPlayer();
  drawHealthBar();
  drawScore();
  drawFireworks();
  if(gameOver) drawGameOverMessage();

  requestAnimationFrame(loop);
}

// ======== AVVIO ========
spawnLevel(currentLevel);
loop();
</script>
</body>
</html>
