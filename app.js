// Global Cytoscape referansı
let cyInstance = null;

const API_BASE_URL = "http://localhost:5008"; // C# API portu

const mockBlockchainData = {
  nodes: [
    { id: "Cuzdan_Efe", label: "Efe Cüzdanı", balance: 100 },
    { id: "Cuzdan_Murat", label: "Murat Cüzdanı", balance: 50 },
    { id: "Cuzdan_Fusun", label: "Füsun Cüzdanı", balance: 30 },
    { id: "Cuzdan_Borsa_Binance", label: "Binance Borsa", balance: 200 },
  ],
  edges: [
    { id: "TX_001_EfeMurat", source: "Cuzdan_Efe", target: "Cuzdan_Murat", amount: 50.0, time: "12:00:15" },
    { id: "TX_002_EfeFusun", source: "Cuzdan_Efe", target: "Cuzdan_Fusun", amount: 30.0, time: "12:05:22" },
    { id: "TX_003_MuratBorsa", source: "Cuzdan_Murat", target: "Cuzdan_Borsa_Binance", amount: 40.0, time: "12:10:45" },
    { id: "TX_004_FusunBorsa", source: "Cuzdan_Fusun", target: "Cuzdan_Borsa_Binance", amount: 20.0, time: "12:15:30" },
  ],
};

const cyStyles = [
  {
    selector: "node",
    style: {
      "background-color": "#27272a",
      label: "data(label)",
      width: "data(size)",
      height: "data(size)",
      color: "#fafafa",
      "font-size": "10px",
      "text-valign": "center",
      "text-halign": "center",
      "text-wrap": "wrap",
      "border-width": 1,
      "border-color": "#52525b",
      "transition-property": "background-color, border-color, border-width",
      "transition-duration": "0.3s"
    },
  },
  {
    selector: "edge",
    style: {
      width: "data(width)",
      "line-color": "#3f3f46",
      "target-arrow-color": "#3f3f46",
      "target-arrow-shape": "triangle",
      "curve-style": "bezier",
      label: "data(amount)",
      "font-size": "9px",
      color: "#a1a1aa",
      "text-margin-y": -10,
      "transition-property": "line-color, target-arrow-color, width",
      "transition-duration": "0.3s"
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
  // --- BFS SARI PARLAMA ANİMASYON STİLİ ---
  {
    selector: ".bfs-highlighted",
    style: {
      "background-color": "#f59e0b",
      "line-color": "#f59e0b",
      "target-arrow-color": "#f59e0b",
      "border-color": "#d97706",
      "border-width": "3px",
    }
  },
  // --- DFS KIRMIZI PARLAMA ANİMASYON STİLİ ---
  {
    selector: ".dfs-highlighted",
    style: {
      "background-color": "#ef4444",
      "line-color": "#ef4444",
      "target-arrow-color": "#ef4444",
      "border-color": "#b91c1c",
      "border-width": "3px",
    }
  }
];

function writeToTerminal(message, type = "info") {
  const term = document.getElementById("ui-terminal");
  if (term) {
    term.innerHTML += `<div class="log-${type}">${message}</div>`;
    term.scrollTop = term.scrollHeight;
  }
}

// Seçim kutularını güncelleyen fonksiyon
function populateSelectBoxes(nodesData) {
  const startSelect = document.getElementById("wallet-start");
  const targetSelect = document.getElementById("wallet-target");

  startSelect.innerHTML = '<option value="">Bir başlangıç cüzdanı seçin...</option>';
  targetSelect.innerHTML = '<option value="">Bir hedef cüzdan seçin...</option>';

  nodesData.forEach((item) => {
    const node = item.data || item;
    const temizIsim = node.label ? node.label.split("\n")[0] : `${node.id.substring(0,8)}...`;
    
    let optionStart = document.createElement("option");
    optionStart.value = node.id;
    optionStart.text = `${node.id.substring(0,12)}... - ${temizIsim}`;
    startSelect.appendChild(optionStart);

    let optionTarget = document.createElement("option");
    optionTarget.value = node.id;
    optionTarget.text = `${node.id.substring(0,12)}... - ${temizIsim}`;
    targetSelect.appendChild(optionTarget);
  });
}

// Grafı ekrana basan fonksiyon
function renderGraph(data) {
  const cyElements = [];

  // Düğümleri (Cüzdanları) ekleme
  data.nodes.forEach((item) => {
    const node = item.data || item;
    const calculatedSize = Math.max(35, Math.min(85, (node.balance || 50) / 2));
    cyElements.push({
      group: "nodes",
      data: {
        id: node.id,
        label: node.label || `${node.id.substring(0,6)}...\n(${node.balance} BTC)`,
        size: calculatedSize,
        balance: node.balance || 0,
      },
    });
  });

  // Kenarları (Transferleri) ekleme
  data.edges.forEach((item) => {
    const edge = item.data || item;
    const calculatedWidth = Math.max(2, Math.min(10, (edge.amount || 10) / 10));
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

  cyInstance = cytoscape({
    container: document.getElementById("cy"),
    elements: cyElements,
    userZoomingEnabled: true,
    style: cyStyles,
    layout: { name: "cose", padding: 50 },
  });

  // Olay Dinleyicileri (Tap Events)
  setupGraphEvents();
}

function setupGraphEvents() {
  const infoBox = document.getElementById("node-info");
  const merkleViewer = document.getElementById("merkle-tree-viewer");

  cyInstance.on("tap", "node", function (evt) {
    const node = evt.target;
    infoBox.innerHTML = `
        <h3>Cüzdan Detayı</h3>
        <p style="word-break: break-all;"><strong>ID:</strong> ${node.id()}</p>
        <p><strong>Bakiye:</strong> ${node.data("balance")} BTC</p>
    `;
  });

  cyInstance.on("tap", "edge", async function (evt) {
    const edge = evt.target;
    infoBox.innerHTML = `
        <h3>Transfer Detayı</h3>
        <p style="word-break: break-all;"><strong>İşlem ID:</strong> ${edge.id()}</p>
        <p style="word-break: break-all;"><strong>Kaynak:</strong> ${edge.data("source")}</p>
        <p style="word-break: break-all;"><strong>Hedef:</strong> ${edge.data("target")}</p>
        <p><strong>Miktar:</strong> ${edge.data("amount")} BTC</p>
        <p><strong>Zaman:</strong> ${edge.data("time")}</p>
    `;

    merkleViewer.innerHTML = `<div class="placeholder-text">Merkle kanıtı doğrulanıyor...</div>`;

    try {
      const response = await fetch(`${API_BASE_URL}/api/wallet/merkle/${edge.id()}`);
      
      if (response.ok) {
        const merkleData = await response.json();
        merkleViewer.innerHTML = `
            <div class="merkle-node merkle-root">Merkle Root<br>[Hash: ${merkleData.root.substring(0, 15)}...]</div>
            <div style="text-align:center; color:#52525b;">↑</div>
            <div class="merkle-node">Durum: ${merkleData.verified ? "Onaylandı ✅" : "Reddedildi ❌"}</div>
            <div style="text-align:center; color:#52525b;">↑</div>
            <div class="merkle-node" style="background:#18181b; border-color:#fafafa; color:#fafafa;">
                <strong>Seçili TX Yaprağı</strong><br>${merkleData.selectedTx.substring(0,20)}...
            </div>
        `;
      } else {
        // Backend'den 404 vs gelirse zorla hataya düşür ki panel sonsuza kadar beklemesin!
        throw new Error(`HTTP Hatası: ${response.status}`);
      }
    } catch (error) {
      merkleViewer.innerHTML = `<div class="placeholder-text" style="color: #ef4444;">Merkle doğrulaması başarısız.<br>İşlem bulunamadı veya sahte!</div>`;
    }
  });
}

// Ana Graf Verisini Çeken Başlangıç Fonksiyonu
async function initSystem() {
  try {
    const response = await fetch(`${API_BASE_URL}/api/wallet`);
    if (response.ok) {
      const data = await response.json();
      populateSelectBoxes(data.nodes);
      renderGraph(data);
      writeToTerminal("> Ana ağ grafiği başarıyla backend sunucusundan yüklendi.", "success");
    } else {
      throw new Error();
    }
  } catch (error) {
    writeToTerminal("[UYARI] Canlı backend hattına bağlanılamadı, Mock Data devrede.", "warning");
    populateSelectBoxes(mockBlockchainData.nodes);
    renderGraph(mockBlockchainData);
  }
}

// --- RAPORA UYGUN DÜĞÜM (NODE) VE KENAR (EDGE) ANİMASYONU ---
async function animatePath(pathArray, algoType) {
  const algoClass = algoType === "bfs" ? "bfs-highlighted" : "dfs-highlighted";
  const algoLabel = algoType.toUpperCase();

  writeToTerminal(`--- ${algoLabel} Takip Operasyonu Başlatıldı ---`, "info");
  cyInstance.elements().removeClass("bfs-highlighted").removeClass("dfs-highlighted");

  for (let i = 0; i < pathArray.length; i++) {
    const currentWalletId = pathArray[i];
    const node = cyInstance.getElementById(currentWalletId);
    
    // Düğümü (Vertex/Node) parlat
    if (node.length > 0) node.addClass(algoClass);

    if (i > 0) {
      const prevWalletId = pathArray[i - 1];
      const prevNode = cyInstance.getElementById(prevWalletId);
      
      if (prevNode.length > 0 && node.length > 0) {
        // İki düğüm arasındaki BÜTÜN kenarları (transferleri) al
        const allEdges = prevNode.edgesTo(node);
        
        if (allEdges.length > 0) {
          // --- MAKSİMUM KAPASİTE FİLTRESİ ---
          let maxEdge = allEdges[0];
          let maxAmount = parseFloat(maxEdge.data("amount")) || 0;

          for (let j = 1; j < allEdges.length; j++) {
            let currentAmount = parseFloat(allEdges[j].data("amount")) || 0;
            if (currentAmount > maxAmount) {
              maxAmount = currentAmount;
              maxEdge = allEdges[j];
            }
          }

          // Sadece en kalın/büyük transfer okunu parlat!
          maxEdge.addClass(algoClass);
          
          const txTime = maxEdge.data("time") || "Bilinmiyor";
          writeToTerminal(`[FON AKIŞI] ${prevWalletId.substring(0,8)}... -> ${currentWalletId.substring(0,8)}... (${maxAmount} BTC) [Saat: ${txTime}]`, "success");
        }
      }
    }
    
    // Katmanlı/Derinlemesine geçiş hissini jüriye yansıtmak için bekleme süresi
    await new Promise((resolve) => setTimeout(resolve, 500)); 
  }
  
  writeToTerminal(`> ${algoLabel} analizi tamamlandı. Ana transfer (Edge) bağlantıları kilitlendi.`, "info");
}

// --- %100 BACKEND ALGORİTMASINA BAĞLI ÇALIŞAN TETİKLEYİCİ ---
async function runAlgorithm(type) {
  const startId = document.getElementById("wallet-start").value;
  const targetId = document.getElementById("wallet-target").value;
  
  // Arayüzdeki minimum transfer inputundan güncel bütçe değerini çekiyoruz
  const minAmount = parseFloat(document.getElementById("min-transfer").value) || 0;

  if (!startId || !targetId) return alert("Lütfen hem Başlangıç hem de Hedef cüzdanı seçin!");
  if (startId === targetId) return alert("Başlangıç ve Hedef cüzdan adresi aynı olamaz!");

  document.getElementById("ui-terminal").innerHTML = "";

  try {
    // URL'in sonuna query parameter olarak minAmount değerini ekliyoruz
    const response = await fetch(`${API_BASE_URL}/api/wallet/${type}/${startId}/${targetId}?minAmount=${minAmount}`);
    
    if (response.ok) {
      const resData = await response.json();
      const path = resData.path || resData.Path;

      if (path && path.length > 0) {
        await animatePath(path, type);
      } else {
        writeToTerminal(`[BİLGİ] C# Algoritması: Belirtilen bütçe limiti (${minAmount} BTC) dahilinde bir fon akış izi bulamadı.`, "warning");
      }
    } else {
      writeToTerminal(`[HATA] Backend sunucusu hata kodu döndürdü: ${response.status}`, "error");
    }
  } catch (error) {
    writeToTerminal("[SİSTEM HATASI] Backend sunucusuna erişilemedi.", "error");
  }
}

// --- YAPAY ZEKA (AI) SENTETİK VERİ ÜRETİMİ ENTEGRASYONU ---
async function generateSyntheticData() {
  const walletCount = parseInt(document.getElementById("ai-wallet-count").value) || 10;
  const txCount = parseInt(document.getElementById("ai-tx-count").value) || 50;

  writeToTerminal(`[AI] Sentetik ağ simülasyonu tetiklendi... (Cüzdan: ${walletCount}, İşlem: ${txCount})`, "warning");
  
  try {
    const response = await fetch(`${API_BASE_URL}/api/AI/sentetik-veri-uret`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ wallet_count: walletCount, transaction_count: txCount })
    });

    if (response.ok) {
      const aiData = await response.json();
      writeToTerminal(`[AI BAŞARILI] ${aiData.message}`, "success");

      const formattedNodes = [];
      const formattedEdges = [];

      const derivedBalances = {};
      aiData.generated_wallets.forEach(addr => derivedBalances[addr] = 100.0); 

      aiData.generated_transactions.forEach(tx => {
        derivedBalances[tx.sender_address] = (derivedBalances[tx.sender_address] || 100) - tx.amount;
        derivedBalances[tx.receiver_address] = (derivedBalances[tx.receiver_address] || 100) + tx.amount;
      });

      aiData.generated_wallets.forEach(addr => {
        const finalBalance = Math.max(5, roundTo(derivedBalances[addr], 4));
        formattedNodes.push({
          id: addr,
          label: `AI_Wallet_${addr.substring(2,6)}\n(${finalBalance} BTC)`,
          balance: finalBalance
        });
      });

      aiData.generated_transactions.forEach(tx => {
        const dateObj = new Date(tx.timestamp * 1000);
        const formattedTime = dateObj.toTimeString().split(' ')[0];

        formattedEdges.push({
          id: tx.tx_id,
          source: tx.sender_address,
          target: tx.receiver_address,
          amount: tx.amount,
          time: formattedTime
        });
      });

      const finalGraphData = { nodes: formattedNodes, edges: formattedEdges };

      populateSelectBoxes(finalGraphData.nodes);
      renderGraph(finalGraphData);
      writeToTerminal("> Yapay zeka sentetik haritası başarıyla render edildi.", "info");

    } else {
      writeToTerminal("[AI HATA] Sunucu sentetik istek hatası döndü.", "error");
    }
  } catch (error) {
    writeToTerminal("[AI BAĞLANTI HATASI] Docker ağındaki Python mikroservisine erişilemedi.", "error");
  }
}

function roundTo(num, decimals) {
  const t = Math.pow(10, decimals);
  return Math.round((num + Number.EPSILON) * t) / t;
}

// --- ETKİNLİK DİNLEYİCİLERİ ---
document.getElementById("btn-bfs").addEventListener("click", () => runAlgorithm("bfs"));
document.getElementById("btn-dfs").addEventListener("click", () => runAlgorithm("dfs"));
document.getElementById("btn-ai-generate").addEventListener("click", generateSyntheticData);

// Kamera Tetikleyicileri
document.getElementById("btn-zoom-in").addEventListener("click", () => {
    if(cyInstance) cyInstance.zoom({ level: cyInstance.zoom() * 1.25, renderedPosition: { x: cyInstance.width()/2, y: cyInstance.height()/2 } });
});
document.getElementById("btn-zoom-out").addEventListener("click", () => {
    if(cyInstance) cyInstance.zoom({ level: cyInstance.zoom() * 0.8, renderedPosition: { x: cyInstance.width()/2, y: cyInstance.height()/2 } });
});
document.getElementById("btn-fit").addEventListener("click", () => {
    if(cyInstance) cyInstance.fit(cyInstance.elements(), 50);
});

// Minimum Limit Filtresi
document.getElementById("min-transfer").addEventListener("input", (e) => {
  if(!cyInstance) return;
  const minAmount = parseFloat(e.target.value) || 0;
  cyInstance.edges().forEach((edge) => {
    if (parseFloat(edge.data("amount")) < minAmount) {
      edge.style("display", "none");
    } else {
      edge.style("display", "element");
    }
  });
});

// Sistemi Ateşle
initSystem();