using System;
using UnityEngine;
using Oxide.Core;
using Oxide.Game.Rust.Cui;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;




namespace Oxide.Plugins
{


    [Info("FeldmannPlugin", "Feldmann", "0.1")]
    public class FeldmannPlugin : RustPlugin
    {
        public static FeldmannPlugin instance;
        Dictionary<String, Loot> loots = new Dictionary<string, Loot>();

        void Loaded()
        {
            instance = this;

            Puts("Carregou Feldmann Plugin!");

            alwaysDay();
            CarregaLoot();
            timer.Once(0.1f, () =>
            {
                int repopulados = 0;
                foreach (var container in UnityEngine.Object.FindObjectsOfType<LootContainer>())
                {
                    if (container == null) continue;

                    //     encheLoot(container);
                    repopulados++;
                }
                Puts("Reenchi " + repopulados + " containers YEEEEAH");


            });

        }
        public void Info(String msg)
        {
            Puts(msg);
        }

        public void DoFileLog(string file, string msg)
        {
            this.LogToFile(file, msg, this, false);
        }

        public void alwaysDay()
        {
            ConVar.Env.time = 12;
            TOD_Sky.Instance.Components.Time.ProgressTime = false;
        }

        //LOOT TABLES
        public void startLootTables()
        {
            /*
             petro = new LootType(1, 2);
             petro.addEntry(createEntry("bandage", 20));
             petro.addEntry(createEntry("syringe.medical", 10));
             petro.addEntry(createEntry("largemedkit", 5));
             comida = new LootType(0, 0);
             comida.addEntry(createEntry(5, new LootItem("apple")));
             comida.addEntry(createEntry(5, new LootItem("granolabar", 1, 1)));
             comida.addEntry(createEntry(5, new LootItem("chocholate")));
             comida.addEntry(createEntry(5, new LootItem("can.tuna")));
             comida.addEntry(createEntry(5, new LootItem("can.beans")));
             comida.addEntry(createEntry(3, new LootItem("waterjug")));
             comida.addEntry(createEntry(3, new LootItem("smallwaterbottle")));
             comida.addEntry(createEntry(2, new LootItem("hammer")));
             comida.addEntry(createEntry(2, new LootItem("building.planner")));
 */
        }


        public static String nomeBarril = "barril";
        public static String nomeBarrilPetroleo = "barrilpetroleo";
        public static String nomeCaixa = "caixa";
        public static String nomeElite = "elite";
        public static String nomeMilitar = "military";
        public static String nomeComida = "comida";
        public static String nomeRoxa = "roxa";
        public static String nomeAirdrop = "airdrop";


        public void CarregaLoot()
        {
            LoadConfig();
            foreach (String s in new String[] { nomeBarril, nomeBarrilPetroleo, nomeCaixa, nomeElite, nomeMilitar, nomeComida, nomeRoxa, nomeAirdrop })
            {
                load(s);
            }


        }


        public Loot load(String oq)
        {
            if (loots.ContainsKey(oq)) return loots[oq];

            if (Config[oq] == null)
            {

                Loot l = new Loot();
                l.maxScrap = 2;
                l.minScrap = 1;
                l.main = new MainLoot();
                l.main.itens.Add(new LootEntry(10, new LootItem(Items.cama)));
                l.main.itens.Add(new LootEntry(10, new LootItem(Armas.waterpipe), new LootItem(Municao.verde, 2, 6)));
                loots.Add(oq, l);
                saveLoot(oq, l);
                return l;
            }

            Dictionary<String, object> dic = Config[oq] as Dictionary<String, object>;
            Loot lt = Loot.fromDictionary(dic);
            return lt;

        }
        public void saveLoot(String nome, Loot l)
        {
            Config[nome] = l.toDictionary();
            SaveConfig();
        }




        public void Enche(LootContainer loot, String nome, int vezes = 1)
        {
            Loot l = load(nome);

            ClearContainer(loot);
            ItemContainer container = loot.inventory;
            Loot ent = load(nome);
            sortAndPut(ent, loot.inventory, vezes);

        }

        void fixZeroAmount(BasePlayer player)
        {
            NextFrame(() =>
            {

                Item[] itens = player.inventory.AllItems();
                for (int x = itens.Length - 1; x >= 0; x--)
                {
                    Item it = itens[x];
                    if (it.amount == 0)
                    {
                        Puts("Removed " + it.info.shortname);
                        it.Remove();
                    }
                }

            });
        }
        #region Events

        //BLOCK CRAFTING
        bool CanCraft(ItemCrafter crafter, ItemBlueprint bp, int amount)
        {

            BasePlayer player = crafter.GetComponent<BasePlayer>();
            player.ChatMessage("Você não pode craftar itens neste servidor!");
            return false;
        }
        //BLOCK RESOURCES
        void OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            if (!entity.ToPlayer()) return;
            if (dispenser.gatherType == ResourceDispenser.GatherType.Flesh)
            {
                if (item.info.category == ItemCategory.Food)
                {
                    return;
                }
            }

            dispenser.enabled = false;
            dispenser.finishBonus.Clear();
            fixZeroAmount(entity.ToPlayer());

            item.amount = 0;

        }
        void OnEntitySpawned(BaseNetworkable entity)
        {
            if (entity == null) return;
            BaseEntity e = entity as BaseEntity;
            if (e == null) return;
            encheLoot(e);

            if (e is BaseCorpse)
            {
                BaseCorpse corpse = e as BaseCorpse;

                ResourceDispenser dis = corpse.GetComponent<ResourceDispenser>();

                if (dis != null)
                {
                    dis.finishBonus.Clear();
                    for (int x = dis.containedItems.Count - 1; x >= 0; x--)
                    {
                        ItemAmount it = dis.containedItems[x];
                        if (it.itemDef.category != ItemCategory.Food)
                        {
                            dis.containedItems.Remove(it);

                        }
                    }
                    dis.Initialize();
                }
            }
        }
        #endregion




        void encheLoot(BaseEntity e)
        {
            string nome = e.ShortPrefabName;
            LootContainer loot = e.GetComponent<LootContainer>();
            if (loot == null) return;
            if (nome.Equals("loot_barrel_1") || nome.Equals("loot_barrel_2") || nome.Equals("loot-barrel-1") || nome.Equals("loot-barrel-2"))
            {
                Enche(loot, nomeBarril);
            }

            if (nome.Equals("oil_barrel") || nome.Equals("crate_normal_2_medical"))
            {
                Enche(loot, nomeBarrilPetroleo);
            }
            if (nome.Equals("crate_normal_2") || nome.Equals("crate_mine") || nome.Equals("minecart"))
            {
                Enche(loot, nomeCaixa);
            }
            if (nome.Equals("crate_elite"))
            {
                Enche(loot, nomeElite);
            }
            if (nome.Equals("crate_normal"))
            {
                Enche(loot, nomeMilitar);
            }
            if (nome.Equals("crate_tools"))
            {
                Enche(loot, nomeRoxa);
            }
            if (nome.Equals("supply_drop"))
            {
                Enche(loot, nomeAirdrop);
            }
            if (nome.Equals("foodbox") || nome.Equals("crate_normal_2_food"))
            {
                Enche(loot, nomeComida);
            }
        }





        //crate_normal_2,crate_mine= caixa quadrada
        //crate_normal = military
        //crate_elite = elite
        //crate_tools = caixa roxa
        //supply_drop = airdrop
        //foodbox = Caixiha de comida



        private void ClearContainer(BaseEntity container)
        {

            if (container is LootContainer)
            {
                (container as LootContainer).minSecondsBetweenRefresh = -1;
                (container as LootContainer).maxSecondsBetweenRefresh = 0;
                (container as LootContainer).CancelInvoke("SpawnLoot");
                if ((container as LootContainer).inventory != null)
                {
                    while ((container as LootContainer).inventory.itemList.Count > 0)
                    {
                        var item = (container as LootContainer).inventory.itemList[0];
                        item.RemoveFromContainer();
                        item.Remove(0f);
                    }
                }
            }
            else
            {
                while ((container as StorageContainer).inventory.itemList.Count > 0)
                {
                    var item = (container as StorageContainer).inventory.itemList[0];
                    item.RemoveFromContainer();
                    item.Remove(0f);
                }
            }
        }


        //Loot Methods
        public LootEntry createEntry(String item, int weight, int minAmount = 1, int maxAmount = 1)
        {
            return new LootEntry(weight, new LootItem(item, minAmount, maxAmount));
        }
        public LootEntry createEntry(int weight, params LootItem[] itens)
        {
            LootEntry l = new LootEntry(weight, itens);
            return l;

        }


        public void addToContainer(ItemContainer cont, LootEntry ent)
        {
            if (ent.randomMode)
            {
                int size = ent.itens.Count;

                int index = UnityEngine.Random.Range(0, ent.itens.Count);
                try
                {
                    if (cont.itemList.Count >= cont.capacity)
                    {
                        cont.capacity = cont.capacity + 1;
                    }

                    ent.itens[index].buildItem().MoveToContainer(cont);
                }
                catch (ArgumentOutOfRangeException ex)
                {

                    Puts("Lista vazia Tem cagada em algum loot!");
                }
            }
            else
            {
                if ((cont.itemList.Count + ent.itens.Count) >= cont.capacity)
                {
                    cont.capacity = cont.itemList.Count + ent.itens.Count;
                }
                foreach (LootItem en in ent.itens)
                {

                    en.buildItem().MoveToContainer(cont);
                }
            }

        }
        #region cmds
        [ConsoleCommand("barril")]
        void cmdEmular(ConsoleSystem.Arg arg)
        {

            simulaBarril(int.Parse(arg.Args[0]));
        }


        [ConsoleCommand("loots")]
        void cmdLoots(ConsoleSystem.Arg arg)
        {

            BaseEntity ob = GameManager.server.CreateEntity("assets/prefabs/misc/supply drop/supply_drop.prefab");
            ob.Spawn();
            SupplyDrop drop = ob.GetComponent<SupplyDrop>();
            LootSpawn loot = drop.lootDefinition;
            Puts("Debugging");
            FeldmannDebug.debugLoot(loot);
            LootContainer cont = ob.GetComponent<LootContainer>();
            Puts("Bla");
            ob.Kill();
            /*IEnumerable<GameObject> enumerable = Enumerable.Cast<GameObject>(FileSystem.Load<ObjectList>("assets/prefabs/misc/supply drop/supply_drop.prefab", true).objects);
            Puts("Heeei o");
            foreach(GameObject go in enumerable){
                FeldmannDebug.debug(go);
            }*/
        }

        [ChatCommand("debug")]
        void cmdDebug(BasePlayer player, string command, string[] args)
        {
            foreach (Loot lt in loots.Values)
            {

                foreach (LootEntry len in lt.main.itens)
                {
                    {
                        foreach (LootItem lit in len.itens)
                        {
                            ItemDefinition it = ItemManager.FindItemDefinition(lit.itemname);
                            if (it == null)
                            {
                                Puts("ITEM: '" + lit.itemname + "' BUGADO");
                            }
                        }
                    }
                }

            };

        }

        #endregion


        public void sortAndPut(Loot sub, ItemContainer cont, int quantos)
        {
            for (int x = 0; x < quantos; x++)
            {
                LootEntry ent = sub.main.sorteia();

                addToContainer(cont, ent);
            }
        }
        #region LootClasses

        public LootEntry createEntry(int peso, String[] itens, String nome = "")
        {
            LootEntry en = new LootEntry(true, peso);
            foreach (String s in itens)
            {
                en.itens.Add(new LootItem(s));
            }
            en.name = nome;
            return en;

        }


        public void simulaBarril(int qtds)
        {
            Dictionary<String, int> itens = new Dictionary<string, int>();
            for (int x = 0; x < qtds; x++)
            {
                LootEntry ent = load("barril").main.sorteia();
                if (!ent.randomMode)
                {
                    foreach (LootItem it in ent.itens)
                    {
                        String nome = it.itemname;
                        int tem = it.sortqtd();
                        if (itens.ContainsKey(nome))
                        {
                            tem += itens[nome];
                        }

                        itens[nome] = tem;
                    }
                }
                else
                {

                    int tem = 1;
                    String nome = ent.name;
                    if (ent.name.Length != 0)
                    {
                        int index = UnityEngine.Random.Range(0, ent.itens.Count);
                        LootItem it = ent.itens[index];

                        nome = it.itemname;
                    }

                    if (itens.ContainsKey(nome))
                    {
                        tem += itens[nome];
                    }

                    itens[nome] = tem;

                }

            }
            List<KeyValuePair<string, int>> list = itens.ToList();
            list.Sort(
                delegate (KeyValuePair<string, int> pair1,
                       KeyValuePair<string, int> pair2)
                {
                    return pair2.Value.CompareTo(pair1.Value);
                }
            );

            foreach (KeyValuePair<String, int> valor in list)
            {
                Puts(valor.Key + ": " + valor.Value);
            }



        }
        public class LootEntry
        {

            public bool randomMode;
            public List<LootItem> itens = new List<LootItem>();
            public int weight;
            public String name = "";

            public LootEntry(int weight, params LootItem[] values)
            {
                if (values != null)
                {
                    foreach (LootItem it in values)
                    {
                        itens.Add(it);
                    }
                }
                this.weight = weight;
                randomMode = false;

            }
            public LootEntry(bool randomMode, int weight, params LootItem[] values)
            {
                if (values != null)
                {
                    foreach (LootItem it in values)
                    {
                        itens.Add(it);
                    }
                }
                this.weight = weight;
                this.randomMode = randomMode;
            }

            public Dictionary<String, object> toDictionary()
            {
                Dictionary<String, object> dic = new Dictionary<string, object>();
                dic.Add("peso", weight);
                dic.Add("random", randomMode);
                List<Dictionary<string, object>> its = new List<Dictionary<string, object>>();
                foreach (LootItem it in itens)
                {
                    its.Add(it.toDictionary());
                }
                dic.Add("itens", its);
                return dic;

            }

            public static LootEntry fromDictionary(Dictionary<string, object> dic)
            {
                int peso = 1;
                bool rand = false;
                if (dic.ContainsKey("rand"))
                {
                    rand = bool.Parse(dic["minAmount"] as string);

                }
                LootEntry ent = new LootEntry(rand, peso);
                if (dic.ContainsKey("itens"))
                {
                    List<Dictionary<string, object>> its = dic["itens"] as List<Dictionary<string, object>>;
                    foreach (Dictionary<string, object> it in its)
                    {
                        ent.itens.Add(LootItem.fromDictionary(it));
                    }


                }
                return ent;
            }

        }
        public class LootItem
        {


            public String itemname;
            int minAmount;
            int maxAmount;
            float minDurability;
            float maxDurability;



            public LootItem(String nome, int minAmount = 1, int maxAmount = 1, float minDurability = 1, float maxDurability = 1)
            {
                this.itemname = nome;
                this.maxAmount = maxAmount;
                this.minAmount = minAmount;
                this.minDurability = minDurability;
                this.maxDurability = maxDurability;
            }

            public Dictionary<String, object> toDictionary()
            {
                Dictionary<String, object> dic = new Dictionary<string, object>();

                dic.Add("item", itemname);
                dic.Add("minAmount", minAmount);
                dic.Add("maxAmount", maxAmount);
                dic.Add("minDurability", minDurability);
                dic.Add("maxDurability", maxDurability);

                return dic;

            }

            public static LootItem fromDictionary(Dictionary<string, object> dic)
            {
                String nome = dic["nome"] as string;
                int minAmount = 1;
                int maxAmount = 1;
                float minDurability = 1;
                float maxDurability = 1;
                int peso = 1;
                if (dic.ContainsKey("minAmount"))
                {
                    minAmount = int.Parse(dic["minAmount"] as string);

                }

                if (dic.ContainsKey("maxAmount"))
                {
                    maxAmount = int.Parse(dic["maxAmount"] as string);
                }
                if (dic.ContainsKey("minDurability"))
                {
                    minDurability = float.Parse(dic["minDurability"] as string);

                }
                if (dic.ContainsKey("maxDurability"))
                {
                    maxDurability = float.Parse(dic["minDurability"] as string);

                }
                LootItem it = new LootItem(nome, minAmount, maxAmount, minDurability, maxDurability);

                return it;
            }

            public int sortqtd()
            {
                return UnityEngine.Random.Range(minAmount, maxAmount + 1);
            }
            public Item buildItem()
            {
                ItemDefinition item = ItemManager.FindItemDefinition(itemname);
                Item it = ItemManager.Create(item, sortqtd());
                //Agua estava vindo sem nada dentro
                if (item.shortname.Equals("waterjug") || item.shortname.Equals("smallwaterbottle"))
                {
                    int waterq = 200;
                    if (item.shortname.Equals("waterjug"))
                    {
                        waterq = 800;
                    }
                    Item water = ItemManager.CreateByName("water", waterq);
                    water.MoveToContainer(it.contents);
                }
                if (it.hasCondition && minDurability < 1)
                {
                    if (maxDurability > 1) maxDurability = 1;
                    float durability = UnityEngine.Random.Range(minDurability, maxDurability);
                    float pct = (it.maxCondition);
                    float cond = pct * durability;
                    it.condition = cond;

                }
                return it;
            }

        }
        #endregion



        public class Loot
        {
            public MainLoot main;
            public int minScrap = 1;
            public int maxScrap = 1;

            public Dictionary<String, object> toDictionary()
            {
                if (main == null) return null;
                Dictionary<String, object> dic = new Dictionary<string, object>();

                dic.Add("minscrap", minScrap);
                dic.Add("maxscrap", maxScrap);
                dic.Add("itens", main.toDictionary());

                return dic;

            }
            public void addScrap(ItemContainer cont)
            {
                Item scrap = getScrap();
                if (scrap != null)
                {
                    scrap.MoveToContainer(cont);
                }
            }
            public Item getScrap()
            {
                if (minScrap == 0 && maxScrap == 0) return null;
                int rnd = UnityEngine.Random.Range(minScrap, maxScrap + 1);
                if (rnd == 0) return null;

                return ItemManager.CreateByName("scrap", rnd);
            }

            public static Loot fromDictionary(Dictionary<string, object> dic)
            {


                MainLoot main = MainLoot.fromDictionary(dic["itens"] as Dictionary<String, object>);
                Loot l = new Loot();
                l.main = main;
                int minScrap = int.Parse(dic["minscrap"] as string);
                int maxScrap = int.Parse(dic["maxscrap"] as string);
                l.minScrap = minScrap;
                l.maxScrap = maxScrap;
                return l;
            }
        }
        public class MainLoot
        {
            public List<LootEntry> itens = new List<LootEntry>();

            void Shuffle<T>(IList<T> ts)
            {
                var count = ts.Count;
                var last = count - 1;
                for (var i = 0; i < last; ++i)
                {
                    var r = UnityEngine.Random.Range(i, count);
                    var tmp = ts[i];
                    ts[i] = ts[r];
                    ts[r] = tmp;
                }
            }

            public Dictionary<String, object> toDictionary()
            {
                Dictionary<String, object> dic = new Dictionary<string, object>();

                List<Dictionary<String, object>> its = new List<Dictionary<String, object>>();
                foreach (LootEntry it in itens)
                {
                    its.Add(it.toDictionary());
                }
                dic.Add("entries", its);



                if (dic.Count == 0) return null;
                return dic;

            }

            public static MainLoot fromDictionary(Dictionary<string, object> dic)
            {
                MainLoot main = new MainLoot();
                if (dic.ContainsKey("entries"))
                {
                    List<Dictionary<String, object>> its = dic["entries"] as List<Dictionary<String, object>>;
                    for (int x = 0; x < its.Count; x++)
                    {
                        main.itens.Add(LootEntry.fromDictionary(its[x]));
                    }
                }



                return main;
            }


            public LootEntry sorteia()
            {

                int soma = 0;
                foreach (LootEntry it in itens)
                {
                    soma += it.weight;
                }
                int rnd = UnityEngine.Random.Range(0, soma);
                for (int i = 0; i < itens.Count; i++)
                {

                    soma -= this.itens[i].weight;
                    if (rnd >= soma)
                    {
                        return this.itens[i];
                    }




                }
                return null;
            }
        }


        #region ItemHelper
        public class Items
        {
            public const String sacoDeDormir = "sleepingbag";
            public const String fechadura = "lock.key";
            public const String armario = "cupboard.tool";
            public const String escada = "ladder.wooden.wall";
            public const String fornalha = "furnace";
            public const String mesaDeReparo = "box.repair.bench";
            public const String fogueira = "campfire";
            public const String bauGrande = "box.wooden.large";
            public const String cordaDeParede = "wall.frame.netting";
            public const String muroDeMadeira = "wall.external.high";
            public const String portaoDeMaderia = "gates.external.high.wood";
            public const String municaoExplosiva = "ammo.rifle.explosive";
            public const String muroDePedra = "wall.external.high.stone";
            public const String portaoDePedra = "gates.external.high.stone";
            public const String cama = "bed";
            public const String portaBlindada = "door.hinged.toptier";
            public const String portaBlidadaDupla = "door.double.hinged.toptier";
            public const String fornalhaGrande = "furnace.large";
            public const String balde = "bucket.water";
            public const String refinaria = "small.oil.refinery";
            public const String maquinaDeVenda = "vending.machine";
            public const String armarioRoupa = "locker";
            public const String dropbox = "dropbox";
            public const String bauPequeno = "box.wooden";
            public const String barrilAgua = "water.barrel";
            public const String estante = "shelves";
            public const String luzBusca = "searchlight";
            public const String caixaCorreio = "mailbox";
            public const String geladeira = "grenade.f1";
            public const String purificador = "water.purifier";
            public const String captadorPequeno = "water.catcher.small";
            public const String captadorGrande = "water.catcher.large";
            public const String trapPeixe = "fishtrap.small";
            public const String codeLock = "lock.code";
            public const String stash = "stash.small";
            public const String airDrop = "supply.signal";


        }
        public class Armas
        {
            public const String eoka = "pistol.eoka";
            public const String m92 = "pistol.m92";
            public const String python = "pistol.python";
            public const String revolver = "pistol.revolver";
            public const String p2 = "pistol.semiauto";
            public const String ak = "rifle.ak";
            public const String bolt = "rifle.bolt";
            public const String lr300 = "rifle.lr300";
            public const String semiauto = "rifle.semiauto";
            public const String m249 = "lgm.m249";
            public const String doubleBarrel = "shotgun.double";
            public const String pump = "shotgun.pump";
            public const String waterpipe = "shotgun.waterpipe";
            public const String bazuca = "rocket.launcher";
            public const String lançaChamas = "flamethrower";
        }

        public class Raid
        {
            public const String explosivoExploracao = "surveycharge";
            public const String C4 = "explosive.timed";
            public const String peidoDeVeia = "explosive.satchel";
            public const String granadaDeFeijao = "grenade.beancan";
            public const String granada = "grenade.f1";
            public const String balaExplosiva = "ammo.rifle.explosive";
            public const String foguete = "ammo.rocket.basic";
            public const String fogueteHv = "ammo.rocket.hv";
            public const String fogueteFire = "ammo.rocket.fire";
        }

        public class Municao
        {
            public const String handmade = "ammo.handmade.shell";
            public const String pistola = "ammo.pistol";
            public const String pistolaFire = "ammo.pistol.fire";
            public const String pistolaHv = "ammo.pistol.hv";
            public const String rifle = "ammo.rifle";
            public const String rifleFire = "ammo.rifle.incendiary";
            public const String rifleHv = "ammo.rifle.hv";
            public const String vermelha = "ammo.shotgun";
            public const String verde = "ammo.shotgun.slug";
            public const String flecha = "arrow.wooden";
            public const String flechaHv = "arrow.hv";

        }


        public class ItemHelperFeld
        {
            public static String[] getShitAttire()
            {
                return new String[]{
                        "burlap.headwrap",
                        "burlap.shirt",
                        "burlap.shoes",
                        "burlap.trousers",
                        "mask.balaclava",
                        "mask.bandana",
                           "hat.beenie",
            "hat.boonie",
            "hat.candle",
            "hat.cap",
            "hat.miner",
            "wood.armor.helmet",
            "pants.shorts",
            "hazmatsuit",
            "bone.armor.suit",
            "deer.skull.mask",
            "jacket",
            "jacket.snow",
            "shirt.collared",
            "attire.hide.poncho",
            "attire.hide.pants",
            "attire.hide.skirt",
            "attire.hide.vest",
            "attire.hide.boots",
            "attire.hide.helterneck",
            "shirt.tanktop",

            };
            }
            public static String[] getOkAttire()
            {
                return new String[]{
            "bucket.helmet",
            "wood.armor.jacket",
            "wood.armor.pants",
            "tshirt",
            "tshirt.long",
            "riot.helmet",
            "shoes.boots",
            "burlap.gloves",
            "pants",
            "hoodie",
            "hat.wolf",

            };
            }

            public static String[] getGoodAttire()
            {
                return new string[]{
            "coffeecan.helmet",
            "roadsign.jacket",
            "roadsign.kilt"

            };
            }
            public static String[] getExcelentAttire()
            {
                return new string[]{
            "metal.facemask",
            "metal.plate.torso",

            };
            }

            public static String[] getVeryExcelentAttire()
            {
                return new string[]{
            "heavy.plate.helmet",
            "heavy.plate.jacket",
            "heavy.plate.pants",
            };

            }




            public static String[] getAmmos()
            {
                return new string[]{
            "ammo.rifle",
            "ammo.rifle.hv",
            "ammo.rifle.incendiary",
            "ammo.pistol",
            "ammo.pistol.fire",
            "ammo.pistol.hv",
            "ammo.shotgun.slug",
            "ammo.shotgun",
            "ammo.handmade.shell"
            };
            }
            public static String[] getTraps()
            {
                return new string[]{
            "trap.bear",
            "trap.landmine",

            };
            }
            public static String[] getMetalDoors()
            {
                return new string[]{
            "door.hinged.metal",
            "door.double.hinged.metal",
            "floor.ladder.hatch",
            "wall.frame.fence.gate",
            "wall.frame.cell.gate",

            };

            }
            public static String[] getWoodenDoors()
            {
                return new string[]{
            "door.hinged.wood",
            "door.double.hinged.wood",
            "wall.frame.shopfront",
            };

            }
            //EU SEI QUE Floor Grill não é parede, mas vou sortear do mesmo jeito foda-se
            public static String[] getParedes()
            {
                return new String[]{
            "wall.frame.fence",
            "wall.frame.cell",
            "wall.frame.shopfront.metal",
            "floor.grill",
            };
            }



            public static String[] getJanelas()
            {
                return new string[]{
            "shutter.metal.embrasure.b",
            "shutter.metal.embrasure.a",
            "wall.window.bars.metal",
            "shutter.wood.a",
            "wall.window.bars.wood",
            "wall.window.bars.toptier",


            };
            }



            public static String[] getLuzes()
            {
                return new String[]{
            "tunalight",
            "lantern",
            "ceilinglight",};
            }
            public static String[] getBarricadas()
            {
                return new string[]{  "barricade.stone",
            "barricade.sandbags",
            "barricade.wood",
            "barricade.woodwire",
            "barricade.metal",
            "barricade.concrete"
            };
            }



            public static String[] GetDecoracao()
            {
                return new string[]{
                "sign.post.town",
                "spinner.wheel",
                "table",
                "chair",
                "jackolantern.angry",
                "jackolantern.happy",
                "sign.wooden.huge",
                "sign.wooden.large",
                "sign.wooden.medium",
                "sign.wooden.small",
                "rug",
                "rug.bear",
                "sign.hanging.ornate",
                "sign.hanging",
                "sign.hanging.banner.large",
                "sign.pole.banner.large",
                "planter.small",
                "planter.large",
                "sign.post.single",
                "sign.post.double",
                "target.reactive",
                "sign.pictureframe.landscape",
                "sign.pictureframe.portrait",
                "sign.pictureframe.tall",
                "sign.pictureframe.xl",
                "sign.pictureframe.xxl",
                "fun.guitar",


            };
            }

            public static String[] getTools(int tier)
            {
                if (tier == 0)
                {
                    return new string[] { "stone.pickaxe", "stonehatchet", "hammer.salvaged" };
                }
                if (tier == 1)
                {
                    return new string[] { "pickaxe", "hatchet", "tool.binoculars" };
                }
                if (tier == 2)
                {
                    return new string[] { "icepick.salvaged", "axe.salvaged" };

                }
                return new string[] { };

            }
            public static String[] getMeleeWeapown(int tier)
            {

                if (tier == 0)
                {
                    return new string[] { "bone.club", "knife.bone", "spear.stone", "spear.wooden", "hammer.salvaged" };
                }
                if (tier == 1)
                {
                    return new string[] { "longsword", "machete", "mace", "salvaged.cleaver", "salvaged.sword" };
                }

                return new string[] { };

            }
            public static String[] getMiras()
            {
                return new string[] { "weapown.mod.holosight", "weapown.mod.simplesight", "weapown.mod.small.scope" };
            }
            public static String[] getMods()
            {
                return new string[] { "weapown.mod.silencer", "weapown.mod.muzzlebrake", "weapown.mod.muzzleboost", "weapown.mod.lasersight", "weapown.mod.flashlight" };
            }



            public static String[] getComponentes()
            {

                return new string[]{
            "propanetank",
            "gears",
            "metalblade",
            "metalpipe",
            "metalspring",
            "riflebody",
            "roadsigns",
            "rope",
            "semibody",
            "sewingkit",
            "sheetmetal",
            "smgbody",
            "tarp",
            "techparts"



            };
            }
        }
        #endregion

        #region Debug
        public class FeldmannDebug
        {
            public static void poorDebug(GameObject go)
            {
                foreach (Transform t in go.transform.GetChildren())
                {
                    FeldmannPlugin.instance.Info(t.gameObject.name);
                }
            }
            public static void drawLine(BasePlayer pl, Vector3 min, Vector3 max)
            {

                pl.SendConsoleCommand("ddraw.line", 120f, Color.blue, min, max);

            }
            static int x = 0;
            public static void drawSphere(BasePlayer pl, params Vector3[] loc)
            {

                Color c = new Color(UnityEngine.Random.Range(0, 255), UnityEngine.Random.Range(0, 255), UnityEngine.Random.Range(0, 255));
                foreach (Vector3 v in loc)
                {
                    pl.SendConsoleCommand("ddraw.text", 120f, c.r + "," + c.g + "," + c.b, v, "x");
                }
            }
            public static void drawBoxCollider(BoxCollider box, BasePlayer pl)
            {
                Vector3 min = box.bounds.min;
                Vector3 max = box.bounds.min;

                float x = max.x - min.x;
                float y = max.y - min.y;
                float z = max.z - min.z;
                pl.Teleport(new Vector3(min.x + (x / 2), min.y + (y / 2), min.z + (z / 2)));

                Vector3 baixoEsquerda = new Vector3(min.x, min.y, min.z);
                Vector3 baixoDireita = new Vector3(min.x + x, min.y, min.z);
                Vector3 cimaEsquerda = new Vector3(min.x, min.y + y, min.z);
                Vector3 cimaDireita = new Vector3(min.x + x, min.y + y, min.z);
                Vector3 baixoEsquerdap = new Vector3(min.x, min.y, min.z + z);
                Vector3 baixoDireitap = new Vector3(min.x + x, min.y, min.z + z);
                Vector3 cimaEsquerdap = new Vector3(min.x, min.y + y, min.z + z);
                Vector3 cimaDireitap = new Vector3(min.x + x, min.y + y, min.z + z);

                drawSphere(pl, baixoEsquerda, baixoDireita, cimaEsquerda, cimaDireita, baixoEsquerdap, baixoDireitap, cimaEsquerdap, cimaDireitap);

                drawLine(pl, baixoEsquerda, baixoDireita);
                drawLine(pl, baixoEsquerda, baixoEsquerdap);
                drawLine(pl, baixoEsquerda, cimaEsquerda);

                drawLine(pl, baixoDireita, cimaDireita);
                drawLine(pl, baixoDireita, baixoDireitap);

                drawLine(pl, cimaEsquerda, cimaDireita);
                drawLine(pl, cimaEsquerda, cimaEsquerdap);
                drawLine(pl, cimaDireita, cimaDireitap);
                drawLine(pl, baixoEsquerdap, baixoDireitap);
                drawLine(pl, baixoEsquerdap, cimaEsquerdap);
                drawLine(pl, cimaEsquerdap, cimaDireitap);
                drawLine(pl, cimaDireitap, baixoDireitap);




            }
            public static void debugMonuments(BasePlayer pl)
            {
                var allobjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                foreach (var gobject in allobjects)
                {
                    if (gobject.name.Contains("autospawn/monument"))
                    {
                        if (gobject.name.Contains("launch_site_1"))
                        {
                            drawBoxColliders(gobject, pl);
                        }
                    }
                }

            }
            public static void debugLoot(LootSpawn loot)
            {
                id = 0;
                debugLootSpawn(loot);


            }
            private static int id = 0;
            private static void debugLootSpawn(LootSpawn loot, int x = 0, int weight = 0)
            {
                String oq = "";
                for (int y = 0; y < x; y++)
                {
                    oq += "  ";
                }
                id++;
                foreach (ItemAmountRanged it in loot.items)
                {
                    FeldmannPlugin.instance.Info(id + " " + oq + weight + "- " + it.itemDef.shortname);
                }
                foreach (LootSpawn.Entry entr in loot.subSpawn)
                {
                    debugLootSpawn(entr.category, x + 1, entr.weight);
                }

            }


            //DEBUG METHODS
            public static void debugComponents(GameObject ob)
            {
                if (ob.GetComponents<Component>().Length == 0)
                {
                    FeldmannPlugin.instance.Info("Nenhum component!");
                    return;
                }
                FeldmannPlugin.instance.Info("====== COMEÇO =====");

                foreach (Component component in ob.GetComponents<Component>())
                {
                    FeldmannPlugin.instance.Info(component.GetType().Name);
                }
                FeldmannPlugin.instance.Info("====== FIM ======");

            }

            public static void drawBoxColliders(GameObject go, BasePlayer pl)
            {
                foreach (Transform t in go.transform.GetChildren())
                {

                    GameObject ob = t.gameObject;

                    BoxCollider box = ob.GetComponent<BoxCollider>();

                    if (box != null)
                    {
                        if (Vector3.Distance(box.bounds.min, box.bounds.max) > 5f)
                        {
                            drawBoxCollider(box, pl);


                        }
                    }
                    drawBoxColliders(t.gameObject, pl);

                }

            }

            public static void debug(GameObject main, int x = 0, String file = null)
            {

                if (x == 0)
                {

                    String compm = "";
                    if (main.GetComponents<Component>().Length > 0)
                    {
                        compm = " (";
                        foreach (Component component in main.GetComponents<Component>())
                        {
                            compm += component.GetType().Name + " - ";
                        }
                        compm += ")";

                    }

                    if (file == null)
                    {
                        FeldmannPlugin.instance.Info("" + main.name + compm);
                    }
                    else
                    {

                        FeldmannPlugin.instance.DoFileLog(file, main.name + compm);
                    }
                }
                String s = "";

                for (int z = 0; z < x; z++) s += "  ";

                foreach (Transform t in main.transform.GetChildren())
                {
                    GameObject ob = t.gameObject;
                    String comp = "";
                    if (ob.GetComponents<Component>().Length > 0)
                    {
                        comp = " (";
                        foreach (Component component in ob.GetComponents<Component>())
                        {
                            comp += component.GetType().Name + " - ";
                        }
                        comp += ")";

                    }

                    if (file == null)
                    {
                        FeldmannPlugin.instance.Info(s + ob.name + comp);
                    }
                    else
                    {

                        FeldmannPlugin.instance.DoFileLog(file, s + ob.name + comp);
                    }
                    if (ob.transform.GetChildren().Count > 0)
                    {
                        debug(ob, x + 1, file);
                    }
                }

            }
        }
        #endregion
    }

}
