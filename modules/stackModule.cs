using MelonLoader;
using ScheduleOne.ObjectScripts;
using HarmonyLib;
using ScheduleOne.ItemFramework;
using ScheduleOne.UI.Shop;
using UnityEngine.Events;
using ScheduleOne.UI.Phone.Delivery;


namespace Modules
{
    internal class stackModule : IModModule
    {
        public void OnModInit()
        {
            MelonLogger.Msg("[StackModule] Initialized");
        }

        public void OnSceneLoaded(int buildIndex, string sceneName)
        {
            if (sceneName != "Main")
            {
                MelonLogger.Msg("[StackModule] Scene not gameplay, disabling.");
                return;
            }
        }

        public void OnUpdate()
        {            
        }

        [HarmonyPatch(typeof(MixingStation), "Start")]
        private class MixingStationPatch
        {
            private static void Postfix(MixingStation __instance)
            {
                __instance.MixTimePerItem = 1;
                __instance.MaxMixQuantity = 80;
            }
        }

        [HarmonyPatch(typeof(DryingRack), "Awake")]
        private class DryingRackPatch
        {
            private static void Postfix(DryingRack __instance)
            {
                __instance.ItemCapacity = 80;
            }
        }

        [HarmonyPatch(typeof(ItemInstance), "get_StackLimit")]
        private class ItemInstanceStackLimitPatch
        {
            private static void Postfix(ItemInstance __instance, ref int __result)
            {
                __result = 80;
            }
        }

        [HarmonyPatch(typeof(ShopInterface), "ListingClicked")]
		public static class ShopInterface_ListingClicked_Patch
        {
            public static bool Prefix(ShopInterface __instance, ListingUI listingUI)
            {
                if (listingUI.Listing.Item.IsPurchasable && listingUI.CanAddToCart())
                {
                    int num = 1;
                    if (__instance.AmountSelector.IsOpen)
                    {
                        num = __instance.AmountSelector.SelectedAmount;
                    }
                    else if (isStackableOffset((ItemDefinition)(object)listingUI.Listing.Item))
                    {
                        num = ((ItemDefinition)listingUI.Listing.Item).StackLimit;
                    }
                    __instance.Cart.AddItem(listingUI.Listing, num);
                    __instance.AddItemSound.Play();
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(CartEntry), "Initialize")]
        public static class CartEntry_Initialize_Patch
        {
            public static void Postfix(CartEntry __instance, Cart cart, ShopListing listing, int quantity)
            {
                ((UnityEventBase)__instance.IncrementButton.onClick).RemoveAllListeners();
                ((UnityEventBase)__instance.DecrementButton.onClick).RemoveAllListeners();
                int offset = 1;
                if (isStackableOffset((ItemDefinition)(object)__instance.Listing.Item))
                {
                    offset = ((ItemDefinition)__instance.Listing.Item).StackLimit;
                }
                ((UnityEvent)__instance.IncrementButton.onClick).AddListener((UnityAction)delegate
                {
                    __instance.Cart.AddItem(__instance.Listing, offset);
                });
                ((UnityEvent)__instance.DecrementButton.onClick).AddListener((UnityAction)delegate
                {
                    __instance.Cart.RemoveItem(__instance.Listing, -offset);
                });
            }
        }

        [HarmonyPatch(typeof(ListingEntry), "Initialize")]
        public static class ListingEntry_Initialize_Patch
        {
            public static void Postfix(ListingEntry __instance, ShopListing match)
            {                
                ((UnityEventBase)__instance.IncrementButton.onClick).RemoveAllListeners();
                ((UnityEventBase)__instance.DecrementButton.onClick).RemoveAllListeners();
                int offset = 1;
                if (isStackableOffset((ItemDefinition)(object)match.Item))
                {
                    offset = ((ItemDefinition)match.Item).StackLimit;
                }
                ((UnityEvent)__instance.IncrementButton.onClick).AddListener((UnityAction)delegate
                {
                    __instance.SetQuantity(__instance.SelectedQuantity + offset, true);
                });
                ((UnityEvent)__instance.DecrementButton.onClick).AddListener((UnityAction)delegate
                {
                    __instance.SetQuantity(__instance.SelectedQuantity - offset, true);
                });
            }
        }

        public static bool isStackableOffset(ItemDefinition item)
        {
            if ((int)item.Category == 1)
            {
                return true;
            }
            if ((int)item.Category == 9)
            {
                return true;
            }
            if ((int)item.Category == 7)
            {
                return true;
            }
            if ((int)item.Category == 2 && !item.Name.Contains("pot") && !item.Name.Equals("Grow Tent"))
            {
                return true;
            }
            if ((int)item.Category == 3 && item.Name.Equals("Trash Bag"))
            {
                return true;
            }
            if ((int)item.Category == 0 && item.Name.Equals("Speed Grow"))
            {
                return true;
            }
            return false;
        }

        private const int DefaultStackSize = 20;

        private const int CustomStackSize = 80;
    }
}
