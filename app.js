// Sunucu kapalıyken patlamamak için fallback datası
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

// API'den güncel graf datasını fetchle
async function getBlockchainData() {
  try {
    const response = await fetch("http://localhost:5008/api/wallet");

    if (response.ok) {
      const gercekVeri = await response.json();
      console.log("DB'den veri çekildi:", gercekVeri);

      // Artık API'den gelen o GERÇEK ve KUSURSUZ veriyi döndürüyoruz!
      return gercekVeri;
    } else {
      throw new Error("API patladı.");
    }
  } catch (error) {
    console.warn("Backend (5008) ayakta değil. Mock data ile devam ediliyor.");
    return mockBlockchainData;
  }
}

// Cytoscape ve UI'ı ayağa kaldıran main fonksiyon
async function initSystem() {
  const data = await getBlockchainData();
  const cyElements = [];

  // Nodeların boyutunu bakiyeye göre dinamik ayarla
  data.nodes.forEach((item) => {
    // API'den geliyorsa item.data içindedir, mock ise direkt item'dır
    const node = item.data || item;

    const calculatedSize = Math.max(30, Math.min(80, node.balance / 2));
    cyElements.push({
      group: "nodes",
      data: {
        id: node.id,
        label: node.label, // C# tarafında BTC yazısını zaten eklemiştik, direkt onu basıyoruz
        size: calculatedSize,
        balance: node.balance,
      },
    });
  });

  // Edgelere (Transferler) miktara göre kalınlık ver
  data.edges.forEach((item) => {
    // API'den geliyorsa item.data içindedir
    const edge = item.data || item;

    const calculatedWidth = Math.max(2, Math.min(10, edge.amount / 10));
    cyElements.push({
      group: "edges",
      data: {
        id: edge.id,
        source: edge.source,
        target: edge.target,
        amount: edge.amount,
        time: edge.time,
        width: calculatedWidth,
      },
    });
  });

  // Graf config ve render (Aşağıdaki kodların tamamı Berke'nin yazdığı orijinal haliyle kalacak)
  const cy = cytoscape({
    container: document.getElementById("cy"),
    elements: cyElements,
    userZoomingEnabled: false, // Scroll zoom iptal (kaybolmayı önlemek için)
    style: [
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
    ],
    layout: { name: "cose", padding: 50 },
  });

  // --- EVENT LISTENERS ---

  // Cüzdana tıklanınca
  cy.on("tap", "node", function (evt) {
    const node = evt.target;

    // Select box'ı senkronize et
    document.getElementById("wallet-search").value = node.id();

    document.getElementById("node-info").innerHTML = `
            <h3>Cüzdan Detayı</h3>
            <p><strong>ID:</strong> ${node.id()}</p>
            <p><strong>Bakiye:</strong> ${node.data("balance")} BTC</p>
        `;

    document.getElementById("merkle-tree-viewer").innerHTML = `
            <div class="placeholder-text">Lütfen graf üzerinden bir transfer işlemi (çizgi) seçin.</div>
        `;
  });

  // Transfer okuna tıklanınca
  cy.on("tap", "edge", function (evt) {
    const edge = evt.target;

    document.getElementById("node-info").innerHTML = `
            <h3>Transfer Detayı</h3>
            <p><strong>İşlem ID:</strong> ${edge.id()}</p>
            <p><strong>Kaynak:</strong> ${edge.data("source")}</p>
            <p><strong>Hedef:</strong> ${edge.data("target")}</p>
            <p><strong>Miktar:</strong> ${edge.data("amount")} BTC</p>
            <p><strong>Zaman:</strong> ${edge.data("time")}</p>
        `;

    // Merkle UI update
    document.getElementById("merkle-tree-viewer").innerHTML = `
            <div class="merkle-node merkle-root">Merkle Root<br>[Hash: 8a9f...2e]</div>
            <div style="text-align:center; color:#52525b;">↑</div>
            <div class="merkle-node">İç Düğüm (Hash L1)</div>
            <div style="text-align:center; color:#52525b;">↑</div>
            <div class="merkle-node" style="background:#18181b; border-color:#fafafa; color:#fafafa;">
                <strong>Seçili TX Yaprağı</strong><br>${edge.id()} Hash
            </div>
        `;
  });

  // Boşluğa tıklayınca seçimleri sıfırla
  cy.on("tap", function (evt) {
    if (evt.target === cy) {
      cy.elements().unselect();
      document.getElementById("wallet-search").value = "";

      document.getElementById("node-info").innerHTML = `
                <h3>Öğe Detayları</h3>
                <p>Graf üzerinde bir düğüme (cüzdan) veya kenara (transfer) tıklayarak detayları görebilirsiniz.</p>
            `;
      document.getElementById("merkle-tree-viewer").innerHTML = `
                <div class="placeholder-text">Lütfen graf üzerinden bir transfer işlemi (çizgi) seçin.</div>
            `;
    }
  });

  // Search inputunu node listesiyle doldur
  const walletSelect = document.getElementById("wallet-search");
  walletSelect.innerHTML = '<option value="">Bir cüzdan seçin...</option>';
  data.nodes.forEach((item) => {
    // API'den gelen paketli veriyi açıyoruz
    const node = item.data || item;

    let option = document.createElement("option");
    option.value = node.id;

    // Label'ın içindeki "\n(10.0 BTC)" kısmını gizleyip sadece cüzdan ismini alalım (Listede daha şık durur)
    const temizIsim = node.label.split("\n")[0];

    option.text = `${node.id} - ${temizIsim}`;
    walletSelect.appendChild(option);
  });

  // Kamera Kontrolleri
  document.getElementById("btn-zoom-in").addEventListener("click", () => {
    cy.zoom({
      level: cy.zoom() * 1.25,
      renderedPosition: { x: cy.width() / 2, y: cy.height() / 2 },
    });
  });

  document.getElementById("btn-zoom-out").addEventListener("click", () => {
    cy.zoom({
      level: cy.zoom() * 0.8,
      renderedPosition: { x: cy.width() / 2, y: cy.height() / 2 },
    });
  });

  document.getElementById("btn-fit").addEventListener("click", () => {
    cy.fit(cy.elements(), 50);
  });

  // BFS Algoritma tetikleyicisi
  document.getElementById("btn-bfs").addEventListener("click", () => {
    const searchId = document.getElementById("wallet-search").value;

    if (!searchId) {
      alert("Lütfen listeden bir cüzdan seçin!");
      return;
    }

    const element = cy.getElementById(searchId);

    if (element.length > 0 && element.isNode()) {
      cy.elements().unselect();
      element.select();
      cy.animate({ center: { eles: element }, zoom: 1.5 }, { duration: 500 });

      document.getElementById("node-info").innerHTML = `
                <h3>Cüzdan Detayı</h3>
                <p><strong>ID:</strong> ${element.id()}</p>
                <p><strong>Bakiye:</strong> ${element.data("balance")} BTC</p>
                <p style="color: #a1a1aa; margin-top: 5px;"><em>[BFS Analizi İçin Başlangıç Noktası Seçildi]</em></p>
            `;
    }
  });

  document.getElementById("btn-dfs").addEventListener("click", () => {
    document.getElementById("btn-bfs").click();
  });
}

initSystem();
