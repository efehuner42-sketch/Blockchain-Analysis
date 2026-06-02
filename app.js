// Sunucu kapalıyken patlamamak için fallback (mock) datası
const mockBlockchainData = {
  nodes: [
    { id: "Cuzdan_Efe", label: "Efe Cüzdanı", balance: 100 },
    { id: "Cuzdan_Murat", label: "Murat Cüzdanı", balance: 50 },
    { id: "Cuzdan_Fusun", label: "Füsun Cüzdanı", balance: 30 },
    { id: "Cuzdan_Borsa_Binance", label: "Binance Borsa", balance: 200 },
  ],
  edges: [
    {
      id: "TX_001_EfeMurat",
      source: "Cuzdan_Efe",
      target: "Cuzdan_Murat",
      amount: 50.0,
      time: "Şimdi",
    },
    {
      id: "TX_002_EfeFusun",
      source: "Cuzdan_Efe",
      target: "Cuzdan_Fusun",
      amount: 30.0,
      time: "Şimdi",
    },
    {
      id: "TX_003_MuratBorsa",
      source: "Cuzdan_Murat",
      target: "Cuzdan_Borsa_Binance",
      amount: 40.0,
      time: "Şimdi",
    },
    {
      id: "TX_004_FusunBorsa",
      source: "Cuzdan_Fusun",
      target: "Cuzdan_Borsa_Binance",
      amount: 20.0,
      time: "Şimdi",
    },
  ],
};

const cyStyles = [
  {
    selector: "node",
    style: {
      "background-color": "#27272a",
      label: "data(label)",
      width: "data(size)", // Berke'nin dinamik boyut hesaplaması eklendi
      height: "data(size)", // Berke'nin dinamik boyut hesaplaması eklendi
      color: "#fafafa",
      "font-size": "10px",
      "text-valign": "center",
      "text-halign": "center",
      "text-wrap": "wrap",
      "border-width": 1,
      "border-color": "#52525b",
    },
  },
  {
    selector: "edge",
    style: {
      width: "data(width)", // Berke'nin dinamik kalınlık hesaplaması eklendi
      "line-color": "#3f3f46",
      "target-arrow-color": "#3f3f46",
      "target-arrow-shape": "triangle",
      "curve-style": "bezier",
      label: "data(amount)",
      "font-size": "9px",
      color: "#a1a1aa",
      "text-margin-y": -10,
    },
  },
  {
    selector: "node:selected",
    style: {
      "border-color": "#fafafa",
      "background-color": "#18181b",
      "border-width": 2,
    },
  },
  {
    selector: "edge:selected",
    style: {
      "line-color": "#fafafa",
      "target-arrow-color": "#fafafa",
    },
  },
  {
    selector: ".highlighted", // Efe'nin animasyon sınıfı korundu
    style: {
      "background-color": "#10b981",
      "line-color": "#10b981",
      "target-arrow-color": "#10b981",
      "transition-property":
        "background-color, line-color, target-arrow-color, border-color",
      "transition-duration": "0.4s",
      "border-color": "#059669",
      "border-width": 2,
    },
  },
];

async function getBlockchainData() {
  try {
    const response = await fetch("http://localhost:5008/api/wallet");
    if (response.ok) {
      const gercekVeri = await response.json();
      console.log("DB'den veri çekildi:", gercekVeri);
      return gercekVeri;
    } else {
      throw new Error("API yanıt vermedi.");
    }
  } catch (error) {
    console.warn("Backend kapalı, Mock Data devrede.");
    return mockBlockchainData;
  }
}

// TERMINAL YAZDIRMA FONKSİYONU (EFE)
function writeToTerminal(message, type = "info") {
  const term = document.getElementById("ui-terminal");
  if (term) {
    term.innerHTML += `<div class="log-${type}">${message}</div>`;
    term.scrollTop = term.scrollHeight;
  }
}

async function initSystem() {
  const data = await getBlockchainData();
  const cyElements = [];

  // Nodeların boyutunu bakiyeye göre dinamik ayarla (BERKE)
  data.nodes.forEach((item) => {
    const node = item.data || item;
    const calculatedSize = Math.max(30, Math.min(80, node.balance / 2));
    cyElements.push({
      group: "nodes",
      data: {
        id: node.id,
        label: node.label,
        size: calculatedSize,
        balance: node.balance,
      },
    });
  });

  // Edgelere (Transferler) miktara göre kalınlık ver (BERKE)
  data.edges.forEach((item) => {
    const edge = item.data || item;
    const calculatedWidth = Math.max(2, Math.min(10, edge.amount / 10));
    cyElements.push({
      group: "edges",
      data: {
        id: edge.id,
        source: edge.source,
        target: edge.target,
        amount: edge.amount,
        time: edge.time || "Bilinmiyor",
        width: calculatedWidth,
      },
    });
  });

  const cy = cytoscape({
    container: document.getElementById("cy"),
    elements: cyElements,
    userZoomingEnabled: false,
    style: cyStyles,
    layout: { name: "cose", padding: 50 },
  });

  const walletSelect = document.getElementById("wallet-search");
  const infoBox = document.getElementById("node-info");
  const merkleViewer = document.getElementById("merkle-tree-viewer");
  const minTransferInput = document.getElementById("min-transfer");

  // Select box'ı temiz isimlerle doldur (BERKE + EFE)
  walletSelect.innerHTML = '<option value="">Bir cüzdan seçin...</option>';
  data.nodes.forEach((item) => {
    const node = item.data || item;
    let option = document.createElement("option");
    option.value = node.id;
    const temizIsim = node.label.split("\n")[0];
    option.text = `${node.id} - ${temizIsim}`;
    walletSelect.appendChild(option);
  });

  // Cüzdana tıklanınca
  cy.on("tap", "node", function (evt) {
    const node = evt.target;
    walletSelect.value = node.id();

    infoBox.innerHTML = `
            <h3>Cüzdan Detayı</h3>
            <p><strong>ID:</strong> ${node.id()}</p>
            <p><strong>Bakiye:</strong> ${node.data("balance")} BTC</p>
        `;
    merkleViewer.innerHTML = `<div class="placeholder-text">Lütfen graf üzerinden bir transfer işlemi seçin.</div>`;
  });

  // Transfer okuna tıklanınca (EFE'nin gerçek API Merkle bağlantısı eklendi)
  cy.on("tap", "edge", async function (evt) {
    const edge = evt.target;
    infoBox.innerHTML = `
            <h3>Transfer Detayı</h3>
            <p><strong>İşlem ID:</strong> ${edge.id()}</p>
            <p><strong>Kaynak:</strong> ${edge.data("source")}</p>
            <p><strong>Hedef:</strong> ${edge.data("target")}</p>
            <p><strong>Miktar:</strong> ${edge.data("amount")} BTC</p>
            <p><strong>Zaman:</strong> ${edge.data("time")}</p>
        `;

    merkleViewer.innerHTML = `<div class="placeholder-text">Merkle kanıtı backend'den çekiliyor...</div>`;

    try {
      const response = await fetch(
        `http://localhost:5008/api/wallet/merkle/${edge.id()}`,
      );
      if (response.ok) {
        const merkleData = await response.json();
        const root = merkleData.root || merkleData.Root;
        const verified =
          merkleData.verified !== undefined
            ? merkleData.verified
            : merkleData.Verified;
        const selectedTx = merkleData.selectedTx || merkleData.SelectedTx;

        merkleViewer.innerHTML = `
                    <div class="merkle-node merkle-root">Merkle Root<br>[Hash: ${root.substring(0, 15)}...]</div>
                    <div style="text-align:center; color:#52525b;">↑</div>
                    <div class="merkle-node">Durum: ${verified ? "Onaylandı ✅" : "Reddedildi ❌"}</div>
                    <div style="text-align:center; color:#52525b;">↑</div>
                    <div class="merkle-node" style="background:#18181b; border-color:#fafafa; color:#fafafa;">
                        <strong>Seçili TX Yaprağı</strong><br>${selectedTx}
                    </div>
                `;
      }
    } catch (error) {
      merkleViewer.innerHTML = `<div class="placeholder-text" style="color: #ef4444;">Backend kapalı, Mock Modu.</div>`;
    }
  });

  // Boşluğa tıklayınca
  cy.on("tap", function (evt) {
    if (evt.target === cy) {
      cy.elements().unselect();
      walletSelect.value = "";
      infoBox.innerHTML = `
                <h3>Öğe Detayları</h3>
                <p>Graf üzerinde bir düğüme (cüzdan) veya kenara (transfer) tıklayarak detayları görebilirsiniz.</p>
            `;
      merkleViewer.innerHTML = `<div class="placeholder-text">Lütfen graf üzerinden bir transfer işlemi (çizgi) seçin.</div>`;
    }
  });

  // Transfer filtresi (EFE)
  if (minTransferInput) {
    minTransferInput.addEventListener("input", (e) => {
      const minAmount = parseFloat(e.target.value) || 0;
      cy.edges().forEach((edge) => {
        if (parseFloat(edge.data("amount")) < minAmount) {
          edge.style("display", "none");
        } else {
          edge.style("display", "element");
        }
      });
      writeToTerminal(
        `[FİLTRE DEĞİŞTİ] Minimum Limit: ${minAmount} BTC`,
        "warning",
      );
    });
  }

  // --- KAMERA KONTROLLERİ ---
  document
    .getElementById("btn-zoom-in")
    .addEventListener("click", () =>
      cy.zoom({
        level: cy.zoom() * 1.25,
        renderedPosition: { x: cy.width() / 2, y: cy.height() / 2 },
      }),
    );
  document
    .getElementById("btn-zoom-out")
    .addEventListener("click", () =>
      cy.zoom({
        level: cy.zoom() * 0.8,
        renderedPosition: { x: cy.width() / 2, y: cy.height() / 2 },
      }),
    );
  document
    .getElementById("btn-fit")
    .addEventListener("click", () => cy.fit(cy.elements(), 50));

  // --- ANİMASYON VE YENİ TERMİNAL MANTIĞI (EFE) ---
  async function animatePath(pathArray, algoName) {
    writeToTerminal(
      `--- ${algoName} Başlatılıyor: ${pathArray[0]} ---`,
      "info",
    );

    cy.elements().removeClass("highlighted");
    cy.animate(
      { fit: { eles: cy.elements(), padding: 50 } },
      { duration: 500 },
    );
    await new Promise((resolve) => setTimeout(resolve, 600));

    let visitedNodes = [];

    for (let i = 0; i < pathArray.length; i++) {
      const currentWalletId = pathArray[i];
      const node = cy.getElementById(currentWalletId);

      if (i === 0) {
        if (node.length > 0) node.addClass("highlighted");
        visitedNodes.push(currentWalletId);
        continue;
      }

      let hasVisibleEdge = false;
      let edgesToHighlight = [];

      for (let j = 0; j < visitedNodes.length; j++) {
        const prevNodeId = visitedNodes[j];
        const prevNode = cy.getElementById(prevNodeId);

        if (prevNode.length > 0 && node.length > 0) {
          const edgesBetween = prevNode.edgesTo(node);

          edgesBetween.forEach((edge) => {
            const miktar = edge.data("amount");
            if (edge.style("display") !== "none") {
              hasVisibleEdge = true;
              edgesToHighlight.push({
                edgeObj: edge,
                prev: prevNodeId,
                amt: miktar,
              });
            } else {
              writeToTerminal(
                `[BLOKE] ${prevNodeId} -> ${currentWalletId} (${miktar} BTC) (Filtreye takıldı)`,
                "error",
              );
            }
          });
        }
      }

      if (hasVisibleEdge) {
        if (node.length > 0) node.addClass("highlighted");
        edgesToHighlight.forEach((item) => {
          item.edgeObj.addClass("highlighted");
          writeToTerminal(
            `${item.prev} -> ${currentWalletId} (${item.amt} BTC)`,
            "success",
          );
        });

        visitedNodes.push(currentWalletId);
        await new Promise((resolve) => setTimeout(resolve, 600));
      }
    }
    writeToTerminal(`> ${algoName} analizi tamamlandı.`, "info");
  }

  async function runAlgorithm(type) {
    const searchId = walletSelect.value;
    const algoName = type === "bfs" ? "BFS" : "DFS";

    if (!searchId) return alert("Lütfen listeden bir cüzdan seçin!");

    document.getElementById("ui-terminal").innerHTML = "";

    const element = cy.getElementById(searchId);
    if (element.length > 0 && element.isNode()) {
      cy.elements().unselect();
      element.select();

      try {
        const response = await fetch(
          `http://localhost:5008/api/wallet/${type}/${searchId}`,
        );
        if (response.ok) {
          const resData = await response.json();
          const algoritmaRotasi = resData.path || resData.Path;

          if (algoritmaRotasi && algoritmaRotasi.length > 0) {
            await animatePath(algoritmaRotasi, algoName);
          }
        }
      } catch (error) {
        writeToTerminal(
          `Backend çevrimdışı. Sistem içi (Mock) test başlatılıyor...`,
          "warning",
        );
        const mockPath = [searchId, "Cuzdan_Murat", "Cuzdan_Borsa_Binance"];
        await animatePath(mockPath, algoName);
      }
    }
  }

  document
    .getElementById("btn-bfs")
    .addEventListener("click", () => runAlgorithm("bfs"));
  document
    .getElementById("btn-dfs")
    .addEventListener("click", () => runAlgorithm("dfs"));
}

initSystem();
