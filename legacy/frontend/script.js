async function carregarTimes() {
  try {
    const response = await fetch("http://localhost:5063/teams");
    const teams = await response.json();
  } catch (error) {
    console.error(error);
  }

  const select1 = document.getElementById("team1");
  const select2 = document.getElementById("team2");

  // limpa antes (importante pra não duplicar)
  select1.innerHTML = "";
  select2.innerHTML = "";

  teams.forEach((team) => {
    const option1 = document.createElement("option");
    option1.value = team.id;
    option1.textContent = team.name;

    const option2 = document.createElement("option");
    option2.value = team.id;
    option2.textContent = team.name;

    select1.appendChild(option1);
    select2.appendChild(option2);
  });
}

window.onload = carregarTimes;

async function simular() {
  const team1 = document.getElementById("team1").value;
  const team2 = document.getElementById("team2").value;

  const response = await fetch(
    `http://localhost:5063/match?team1Id=${team1}&team2Id=${team2}`,
    {
      method: "POST",
    },
  );

  container.innerHTML = "Simulando partida...";

  const data = await response.json();

  mostrarResultado(data);
}

function mostrarResultado(data) {
  let container = document.getElementById("resultado");

  // limpa antes (importante pra repetir partidas)
  container.innerHTML = "";

  // placar
  const placar = document.createElement("h2");
  placar.innerText = `${data.team1} ${data.score} ${data.team2}`;

  container.appendChild(placar);

  // gols
  data.goals.forEach((gol) => {
    const p = document.createElement("p");
    p.innerText = `${gol.minute}' ${gol.player} (${gol.team})`;
    container.appendChild(p);
  });
}
