using System;
using EVEMon.Common.Extensions;
using System.Runtime.Serialization;
using EVEMon.Common.Enumerations.CCPAPI;
using EVEMon.Common.Serialization.Eve;
using System.Globalization;

namespace EVEMon.Common.Serialization.Esi
{
    [DataContract]
    public sealed class EsiWalletJournalListItem
    {
        private DateTime date;
        private EsiRefTypeString refType;

        public EsiWalletJournalListItem()
        {
            date = DateTime.MinValue;
            refType = EsiRefTypeString.none;
        }

        [DataMember(Name = "ref_id")]
        public long ID { get; set; }

        // Can be one of 117 items, see https://gist.github.com/ccp-zoetrope/c03db66d90c2148724c06171bc52e0ec
        // to convert to integer
        [DataMember(Name = "ref_type")]
        private string RefTypeJson
        {
            get
            {
                return refType.ToString().ToLower();
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    Enum.TryParse(value, true, out refType);
            }
        }

        [IgnoreDataMember]
        public int RefTypeID
        {
            get
            {
                return (int)refType;
            }
        }

        [DataMember(Name = "first_party_id", EmitDefaultValue = false, IsRequired = false)]
        public long OwnerID1 { get; set; }
        
        [DataMember(Name = "second_party_id", EmitDefaultValue = false, IsRequired = false)]
        public long OwnerID2 { get; set; }
        
        [DataMember(Name = "amount", IsRequired = false)]
        public decimal Amount { get; set; }

        [DataMember(Name = "balance", IsRequired = false)]
        public decimal Balance { get; set; }

        [DataMember(Name = "description", EmitDefaultValue = false, IsRequired = false)]
        public string Description { get; set; }

        [DataMember(Name = "reason", EmitDefaultValue = false, IsRequired = false)]
        public string Reason { get; set; }

        [DataMember(Name = "tax_receiver_id", EmitDefaultValue = false, IsRequired = false)]
        public long TaxReceiverID { get; set; }

        [DataMember(Name = "tax", EmitDefaultValue = false, IsRequired = false)]
        public decimal TaxAmount { get; set; }

        [DataMember(Name = "extra_info", EmitDefaultValue = false, IsRequired = false)]
        public EsiWalletJournalExtra Extra { get; set; }

        [DataMember(Name = "date")]
        private string DateJson
        {
            get { return date.DateTimeToTimeString(); }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    date = value.TimeStringToDateTime();
            }
        }

        [IgnoreDataMember]
        public DateTime Date
        {
            get
            {
                return date;
            }
        }
        
        public SerializableWalletJournalListItem ToXMLItem()
        {
            // This is never actually used in EveMon!
            string argName1 = string.Empty;
            long argId1 = 0L;
            var extraData = Extra;

            if (extraData != null)
            {
                // Populate arguments from the extra data based on the ref type
                // See http://eveonline-third-party-documentation.readthedocs.io/en/latest/xmlapi/constants.html#reference-type
                switch (refType)
                {
                    case EsiRefTypeString.player_trading:
                        argId1 = extraData.LocationID;
                        break;
                    case EsiRefTypeString.market_transaction:
                        argName1 = extraData.TransactionID.ToString(CultureInfo.InvariantCulture);
                        break;
                    case EsiRefTypeString.office_rental_fee:
                    case EsiRefTypeString.brokers_fee:
                    case EsiRefTypeString.jump_clone_installation_fee:
                    case EsiRefTypeString.jump_clone_activation_fee:
                    case EsiRefTypeString.reprocessing_tax:
                        argName1 = "EVE System";
                        argId1 = 1L;
                        break;
                    case EsiRefTypeString.bounty_prize:
                        argName1 = extraData.NpcName;
                        argId1 = extraData.NpcID;
                        break;
                    case EsiRefTypeString.insurance:
                        argName1 = extraData.DestroyedShipTypeID.ToString(CultureInfo.InvariantCulture);
                        break;
                    case EsiRefTypeString.agent_mission_reward:
                    case EsiRefTypeString.agent_mission_time_bonus_reward:
                    case EsiRefTypeString.cspa:
                    case EsiRefTypeString.corporation_account_withdrawal:
                    case EsiRefTypeString.medal_creation:
                    case EsiRefTypeString.medal_issued:
                        argId1 = extraData.CharacterID;
                        break;
                    case EsiRefTypeString.corporation_logo_change_cost:
                        argId1 = extraData.CorporationID;
                        break;
                    case EsiRefTypeString.alliance_maintainance_fee:
                        argId1 = extraData.AllianceID;
                        break;
                    case EsiRefTypeString.manufacturing:
                        argName1 = extraData.JobID.ToString(CultureInfo.InvariantCulture);
                        break;
                    case EsiRefTypeString.contract_auction_bid:
                    case EsiRefTypeString.contract_auction_bid_refund:
                    case EsiRefTypeString.contract_price:
                    case EsiRefTypeString.contract_brokers_fee:
                    case EsiRefTypeString.contract_sales_tax:
                    case EsiRefTypeString.contract_deposit:
                    case EsiRefTypeString.contract_price_payment_corp:
                    case EsiRefTypeString.contract_brokers_fee_corp:
                    case EsiRefTypeString.contract_deposit_corp:
                    case EsiRefTypeString.contract_deposit_refund:
                        argName1 = extraData.ContractID.ToString(CultureInfo.InvariantCulture);
                        break;
                    case EsiRefTypeString.bounty_prizes:
                        argId1 = extraData.SystemID;
                        break;
                    case EsiRefTypeString.planetary_import_tax:
                    case EsiRefTypeString.planetary_export_tax:
                        argId1 = extraData.PlanetID;
                        // Planet name available from geography? But no planets in the geo file
                        break;
                    case EsiRefTypeString.industry_job_tax:
                        argId1 = extraData.LocationID;
                        break;
                    default:
                        // Empty
                        break;
                }
            }

            return new SerializableWalletJournalListItem()
            {
                Amount = Amount,
                ArgID1 = argId1,
                ArgName1 = argName1,
                Balance = Balance,
                Date = Date,
                ID = ID,
                Reason = Reason,
                OwnerID1 = OwnerID1,
                OwnerID2 = OwnerID2,
                RefTypeID = RefTypeID,
                TaxAmount = TaxAmount,
                TaxReceiverID = TaxReceiverID
            };
        }
    }
}
