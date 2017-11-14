using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;

using UnityEngine;

namespace Oxide.Plugins {
    [Info("FeldmannShop", "Feldmann", "0.1")]
    class FeldmannShop : RustPlugin {

        private const string ShopOverlayName = "ShopOverlay";
        private const string ItemOverlay = "ItemOverlay";
        private const string SucataName = "SucataOverlay";

        int[] compras = new int[] { 1, 10, 100, -1 };
        List<ShopInfo> shops = new List<ShopInfo>();


        void startShop() {
            shops.Clear();
            shops.Add(new ShopInfo("Madeira", "wood", 4, "http://i.imgur.com/3Inagwi.png"));
            shops.Add(new ShopInfo("Pedra", "stones", 2, "http://i.imgur.com/Ull10CY.png"));
            shops.Add(new ShopInfo("Metal", "metal.fragments", 1, "http://i.imgur.com/ckmMX8z.png"));
        }

        void onInit() {
            startShop();
        }
        static int CurrentTime() { return Facepunch.Math.Epoch.Current; }

        private static CuiElementContainer CreateShopOverlay() {
            return new CuiElementContainer
            {

                {
                    new CuiPanel
                    {
                        Image = {Color = "0.1 0.1 0.1 0.8"},
                        RectTransform = {AnchorMin = "0.2 0.2", AnchorMax = "0.8 0.8"},
                        CursorEnabled = true
                    },
                    new CuiElement().Parent,
                    ShopOverlayName
                },

                {
                    new CuiLabel
                    {
                        Text = {Text = "Shop de Recursos", FontSize = 30, Align = TextAnchor.MiddleCenter},
                        RectTransform = {AnchorMin = "0.3 0.9", AnchorMax = "0.7 0.99"}
                    },
                    ShopOverlayName
                },
                {
                    new CuiLabel {
                        Text = { Text = "www.feldmannjr.me", FontSize = 13, Align = TextAnchor.MiddleCenter },
                        RectTransform = { AnchorMin = "0.3 0.87", AnchorMax = "0.7 0.91" }
                    },
                    ShopOverlayName
                },
                {
                    new CuiLabel {
                        Text = { Text = "Troque Sucata por recursos!", FontSize = 22, Align = TextAnchor.LowerCenter },
                        RectTransform = { AnchorMin = "0.0 0.80", AnchorMax = "1 0.87" }
                    },
                    ShopOverlayName

                },

                {
                    new CuiButton
                    {
                        Button = {Command ="shop.close", Color = "0.698 0.13 0.13 1"},
                        RectTransform = {AnchorMin = "0.4 0.03", AnchorMax = "0.6 0.1"},
                        Text = {Text = "Fechar", FontSize = 20, Align = TextAnchor.MiddleCenter}
                    },
                    ShopOverlayName
                }
            };
        }
        private static CuiElementContainer CreateScraps() {
            return new CuiElementContainer {

            };
        }
        public class ShopInfo {
            public String url;
            public float troca;
            public String nome;
            public String item;
            public ShopInfo(String nome, String item, float troca, String url) {
                this.nome = nome;
                this.url = url;
                this.troca = troca;
                this.item = item;
            }


        }
        CuiPanel buildRecurso(int x, ShopInfo info) {
            PosInfo pos = getInfo(x);
            CuiPanel pn = new CuiPanel {
                Image = { Color = "0.729 0.729 0.729 0.5" },
                RectTransform = { AnchorMin = pos.minX + " " + pos.minY, AnchorMax = pos.maxX + " " + pos.maxY },
                CursorEnabled = true
            };
            return pn;
        }
        public PosInfo getInfo(int slot) {
            PosInfo info = new PosInfo();
            float sobra = 1 - (0.22f * 3) + 0.02f;
            float minX = sobra / 2;
            float divisor = 16f / 9f;
            minX += (0.22f) * slot;
            float maxX = minX + 0.20f;
            float minY = 0.30f;
            float maxY = minY + (divisor * 0.2f);
            info.minX = minX;
            info.maxX = maxX;
            info.minY = minY;
            info.maxY = maxY;
            return info;
        }

        public class PosInfo {
            public float minX;
            public float minY;
            public float maxX;
            public float maxY;
        }

        public const String ScrapUrl = "http://i.imgur.com/V7ZluVS.png";
        public const String ScrapUrlCortada = "http://i.imgur.com/ZRWwAj0.png";
        Dictionary<BasePlayer, CuiElementContainer> conts = new Dictionary<BasePlayer, CuiElementContainer>();
        void ShowShop(BasePlayer player) {
            if(shops.Count == 0) {
                startShop();
            }
            DestroyUi(player, true);
            int tem = getScraps(player);
            CuiElementContainer container;


            container = CreateShopOverlay();

            for(int x = 0; x < 3; x++) {
                ShopInfo info = shops[x];
                if(info == null) continue;

                CuiPanel slot = buildRecurso(x, info);
                container.Add(slot, ShopOverlayName, ItemOverlay + x);

                container.Add(new CuiElement {
                    Parent = ItemOverlay + x,
                    Components ={
                        new CuiRawImageComponent{
                            Url=info.url,
                            Sprite = "assets/content/textures/generic/fulltransparent.tga",
                            Color = "0.8 0.8 0.8 1" ,

                        },
                        new CuiRectTransformComponent { }
                    }

                });
                PosInfo pos = getInfo(x);
                float minY = pos.maxY + 0.05f;
                float maxY = minY + 0.08f;
                //NOME EM CIMA
                container.Add(new CuiLabel {
                    Text = { Text = info.nome, FontSize = 20, Align = TextAnchor.LowerCenter, Color = "1 0.745 0.039 1" },
                    RectTransform = { AnchorMin = pos.minX + " " + minY, AnchorMax = pos.maxX + " " + maxY }

                }, ShopOverlayName);
                minY = pos.maxY + 0.005f;
                maxY = minY + 0.08f;
                //Compre por
                container.Add(new CuiLabel {
                    Text = { Text = info.troca + " por  ", FontSize = 18, Align = TextAnchor.LowerCenter },
                    RectTransform = { AnchorMin = pos.minX + " " + minY, AnchorMax = pos.maxX + " " + maxY }

                }, ShopOverlayName);
                //Icone sucata
                float tamanho = 0.025f;
                float minX = (pos.maxX - pos.minX) * 0.60f;
                minX += pos.minX;
                float maxX = (minX) + (tamanho);
                minY = pos.maxY + 0.0085f;
                maxY = minY + tamanho * (16f / 9f);
                container.Add(new CuiElement {
                    Parent = ShopOverlayName,
                    Components ={
                        new CuiRawImageComponent{
                            Url=ScrapUrl,
                            Sprite = "assets/content/textures/generic/fulltransparent.tga",
                            Color = "0.8 0.8 0.8 1" ,

                        },
                        new CuiRectTransformComponent {
                            AnchorMin = minX+" "+minY,
                            AnchorMax = maxX+" "+maxY,
                        }
                    }
                });

                //BOTOES DE COMPRAR
                for(int y = 0; y < compras.Length; y++) {

                    addBuyButton(container, pos, info, y, tem);
                }


            }

            CuiHelper.AddUi(player, container);
            updateSucata(player);

        }

        public float convert(float f) {
            return 0.2f + (f * 0.6f);
        }
        public void updateSucata(BasePlayer pl) {
            CuiHelper.DestroyUi(pl, SucataName);
            CuiElementContainer scrapcont = CreateScraps();
            CuiLabel scraps = new CuiLabel {
                Text = { Text = "Você tem <color=yellow>" + getScraps(pl) + "</color> sucatas!", FontSize = 18, Align = TextAnchor.LowerCenter },
                RectTransform = { AnchorMin = convert(0) + " " + convert(0.15f), AnchorMax = convert(1f) + " " + convert(0.2f) }
            };
            scrapcont.Add(scraps, "Hud", SucataName);
            CuiHelper.AddUi(pl, scrapcont);
        }

        public bool canUse(BasePlayer pl) {
            if(!pl.CanBuild()) return false;
            return true;
        }


        //Que metodo tesudo
        public PosInfo getPosToButtons(PosInfo shoppos, int x) {
            PosInfo info = new PosInfo();
            float espaco = 0.02f;
            float qtd = compras.Length;
            float qtd1 = qtd - 1f;
            float espacos = espaco * qtd1;
            float porBotao = (1 - espacos) / qtd;
            float minX = porBotao * x;
            minX += (espaco * x);
            float maxX = minX + porBotao;
            float difX = shoppos.maxX - shoppos.minX;
            float realMinX = shoppos.minX + (difX * minX);
            float realMaxX = shoppos.minX + (difX * maxX);
            float maxY = shoppos.minY - 0.01f;
            float minY = maxY - 0.06f;
            info.minX = realMinX;
            info.maxX = realMaxX;
            info.minY = minY;
            info.maxY = maxY;
            return info;
        }

        public String getColor(int scrap, int tem) {
            if(scrap == -1 && tem >= 1) {
                return "1 0.968 0.121";
            }
            if(tem < scrap || scrap == -1) {
                return "0.639 0 0.082";
            }
            return "0.211 0.988 0.286";
        }


        public void addBuyButton(CuiElementContainer cont, PosInfo shoppos, ShopInfo shop, int x, int tem) {
            PosInfo pos = getPosToButtons(shoppos, x);

            int scrap = compras[x];


            CuiPanel pn = new CuiPanel {
                Image = {
                    Color = getColor(scrap,tem)+" 1.0" ,

                },
                RectTransform ={   AnchorMin = pos.minX+" "+pos.minY,
                    AnchorMax = pos.maxX+" "+pos.maxY}

            };
            cont.Add(pn, ShopOverlayName);
            CuiElement icone = new CuiElement {
                Parent = ShopOverlayName,
                Components ={
                    new CuiRawImageComponent{
                        Url=ScrapUrlCortada,
                        Sprite = "assets/content/textures/generic/fulltransparent.tga",
                        Color = getColor(scrap,tem)+" 0.5" ,

                    },
                    new CuiRectTransformComponent {
                        AnchorMin = pos.minX+" "+pos.minY,
                        AnchorMax = pos.maxX+" "+pos.maxY,
                    }
                }
            };

            cont.Add(icone);
            String color = "1 1 1 0";
            String oq = "" + compras[x]; ;
            if(oq.Equals("-1")) {
                oq = "Tudo";
            }
            var button = new CuiButton {
                Button = { Command = "shop.buy " + shop.item + " " + compras[x], Color = color },
                RectTransform = { AnchorMin = pos.minX + " " + pos.minY, AnchorMax = pos.maxX + " " + pos.maxY },
                Text = { Text = " " }
            };

            var ce = new CuiElement {
                Parent = ShopOverlayName,
                Components ={

                    new CuiTextComponent{
                        Text = oq,
                        Align = TextAnchor.MiddleCenter,

                    },
                    new CuiOutlineComponent{
                        Distance="1.0 1.0",
                        Color="0.0 0.0 0.0 1"

                    }
                    ,
                    new CuiRectTransformComponent {
                        AnchorMin = pos.minX+" "+pos.minY,
                        AnchorMax = pos.maxX+" "+pos.maxY,
                    },

                }

            };

            cont.Add(ce);
            cont.Add(button, ShopOverlayName);
        }



        void DestroyUi(BasePlayer player, bool full = false) {

            CuiHelper.DestroyUi(player, ShopOverlayName);
            CuiHelper.DestroyUi(player, SucataName);
        }
        [ChatCommand("shop")]
        void cmdShop(BasePlayer player, string command, string[] args) {

            if(!canUse(player)) {
                SendReply(player, "Você não pode usar o shop aqui!");
                return;
            }
            ShowShop(player);
        }
        [ConsoleCommand("shop.close")]
        void cmdShopClose(ConsoleSystem.Arg arg) {
            if(arg.Player() != null) {
                DestroyUi(arg.Player());
            }
        }
        [ConsoleCommand("shop.buy")]
        void cmdShopBuy(ConsoleSystem.Arg arg) {
            if(arg.HasArgs(2)) {

                BasePlayer pl = arg.Player();
                if(pl == null) return;
                if(!canUse(pl)) return;
                String item = arg.Args[0];
                String qtds = arg.Args[1];
                int qtd = Convert.ToInt32(qtds);
                if(qtd == -1) {
                    qtd = getScraps(pl);
                }
                int scraps = getScraps(pl);
                if(scraps < qtd || scraps == 0) {
                    pl.ChatMessage("Você não tem esta quantidade de sucata!");
                    return;
                }
                ShopInfo shop = null;
                foreach(ShopInfo info in shops) {
                    if(info.item.Equals(item)) {
                        shop = info;
                    }
                }
                if(shop == null) return;

                ItemDefinition def = ItemManager.FindItemDefinition(shop.item);
                int max = def.stackable;
                int giveQtd = (int)shop.troca * qtd;
                int qtdCon = giveQtd;
                pl.inventory.Take(null, ItemManager.FindItemDefinition("scrap").itemid, qtd);
                if(giveQtd <= max) {
                    pl.GiveItem(ItemManager.Create(def, giveQtd));
                } else {
                    for(int x = giveQtd; x > 0; x -= max) {
                        int give = x >= max ? max : x;
                        pl.GiveItem(ItemManager.Create(def, give));
                    }

                }
                updateSucata(pl);

                pl.ChatMessage("Você comprou " + qtdCon + " " + shop.nome + "!");


            }
        }

        public int getScraps(BasePlayer pl) {
            int scraps = 0;
            foreach(Item it in pl.inventory.AllItems()) {
                if(it.info.shortname.Equals("scrap")) {
                    scraps += it.amount;
                }
            }
            return scraps;
        }



    }
}
