// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: Views.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Budget.Data.Views {

  /// <summary>Holder for reflection information generated from Views.proto</summary>
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public static partial class ViewsReflection {

    #region Descriptor
    /// <summary>File descriptor for Views.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static ViewsReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CgtWaWV3cy5wcm90bxIVc2VxdWVudGlhbHJlYWQuYnVkZ2V0GgpEYXRhLnBy",
            "b3RvIq4BCg9UcmFuc2FjdGlvbkxpc3QSMQoIcGFnZUluZm8YASABKAsyHy5z",
            "ZXF1ZW50aWFscmVhZC5idWRnZXQuUGFnZUluZm8SLgoHZmlsdGVycxgCIAMo",
            "CzIdLnNlcXVlbnRpYWxyZWFkLmJ1ZGdldC5GaWx0ZXISOAoMdHJhbnNhY3Rp",
            "b25zGAMgAygLMiIuc2VxdWVudGlhbHJlYWQuYnVkZ2V0LlRyYW5zYWN0aW9u",
            "Io4BChNVbnNvcnRlZFRyYW5zYWN0aW9uEjcKC3RyYW5zYWN0aW9uGAEgASgL",
            "MiIuc2VxdWVudGlhbHJlYWQuYnVkZ2V0LlRyYW5zYWN0aW9uEj4KCmNhdGVn",
            "b3JpZXMYAyADKAsyKi5zZXF1ZW50aWFscmVhZC5idWRnZXQuVHJhbnNhY3Rp",
            "b25DYXRlZ29yeSJKCghQYWdlSW5mbxITCgtjdXJyZW50UGFnZRgBIAEoDRIR",
            "CglwYWdlQ291bnQYAiABKA0SFgoOcmVzdWx0c1BlclBhZ2UYAyABKA0iWgoG",
            "RmlsdGVyEkEKD2RhdGVSYW5nZUZpbHRlchgBIAEoCzImLnNlcXVlbnRpYWxy",
            "ZWFkLmJ1ZGdldC5EYXRlUmFuZ2VGaWx0ZXJIAEINCgtmaWx0ZXJfdHlwZSIt",
            "Cg9EYXRlUmFuZ2VGaWx0ZXISDQoFc3RhcnQYASABKAMSCwoDZW5kGAIgASgD",
            "Io0BChBSZXBvcnREYXRhU2VyaWVzEi4KB2ZpbHRlcnMYASADKAsyHS5zZXF1",
            "ZW50aWFscmVhZC5idWRnZXQuRmlsdGVyEhEKCWZyZXF1ZW5jeRgCIAEoAxI2",
            "CgZzZXJpZXMYAyADKAsyJi5zZXF1ZW50aWFscmVhZC5idWRnZXQuUmVwb3J0",
            "RGF0YUdyb3VwIncKD1JlcG9ydERhdGFHcm91cBIuCgdmaWx0ZXJzGAEgAygL",
            "Mh0uc2VxdWVudGlhbHJlYWQuYnVkZ2V0LkZpbHRlchI0CgRkYXRhGAIgAygL",
            "MiYuc2VxdWVudGlhbHJlYWQuYnVkZ2V0LlJlcG9ydERhdGFQb2ludCJeCg9S",
            "ZXBvcnREYXRhUG9pbnQSPAoIY2F0ZWdvcnkYASABKAsyKi5zZXF1ZW50aWFs",
            "cmVhZC5idWRnZXQuVHJhbnNhY3Rpb25DYXRlZ29yeRINCgVjZW50cxgCIAEo",
            "EUIUqgIRQnVkZ2V0LkRhdGEuVmlld3NiBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Budget.Data.Data.DataReflection.Descriptor, },
          new pbr::GeneratedCodeInfo(null, new pbr::GeneratedCodeInfo[] {
            new pbr::GeneratedCodeInfo(typeof(global::Budget.Data.Views.TransactionList), global::Budget.Data.Views.TransactionList.Parser, new[]{ "PageInfo", "Filters", "Transactions" }, null, null, null),
            new pbr::GeneratedCodeInfo(typeof(global::Budget.Data.Views.UnsortedTransaction), global::Budget.Data.Views.UnsortedTransaction.Parser, new[]{ "Transaction", "Categories" }, null, null, null),
            new pbr::GeneratedCodeInfo(typeof(global::Budget.Data.Views.PageInfo), global::Budget.Data.Views.PageInfo.Parser, new[]{ "CurrentPage", "PageCount", "ResultsPerPage" }, null, null, null),
            new pbr::GeneratedCodeInfo(typeof(global::Budget.Data.Views.Filter), global::Budget.Data.Views.Filter.Parser, new[]{ "DateRangeFilter" }, new[]{ "FilterType" }, null, null),
            new pbr::GeneratedCodeInfo(typeof(global::Budget.Data.Views.DateRangeFilter), global::Budget.Data.Views.DateRangeFilter.Parser, new[]{ "Start", "End" }, null, null, null),
            new pbr::GeneratedCodeInfo(typeof(global::Budget.Data.Views.ReportDataSeries), global::Budget.Data.Views.ReportDataSeries.Parser, new[]{ "Filters", "Frequency", "Series" }, null, null, null),
            new pbr::GeneratedCodeInfo(typeof(global::Budget.Data.Views.ReportDataGroup), global::Budget.Data.Views.ReportDataGroup.Parser, new[]{ "Filters", "Data" }, null, null, null),
            new pbr::GeneratedCodeInfo(typeof(global::Budget.Data.Views.ReportDataPoint), global::Budget.Data.Views.ReportDataPoint.Parser, new[]{ "Category", "Cents" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class TransactionList : pb::IMessage<TransactionList> {
    private static readonly pb::MessageParser<TransactionList> _parser = new pb::MessageParser<TransactionList>(() => new TransactionList());
    public static pb::MessageParser<TransactionList> Parser { get { return _parser; } }

    public static pbr::MessageDescriptor Descriptor {
      get { return global::Budget.Data.Views.ViewsReflection.Descriptor.MessageTypes[0]; }
    }

    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    public TransactionList() {
      OnConstruction();
    }

    partial void OnConstruction();

    public TransactionList(TransactionList other) : this() {
      PageInfo = other.pageInfo_ != null ? other.PageInfo.Clone() : null;
      filters_ = other.filters_.Clone();
      transactions_ = other.transactions_.Clone();
    }

    public TransactionList Clone() {
      return new TransactionList(this);
    }

    /// <summary>Field number for the "pageInfo" field.</summary>
    public const int PageInfoFieldNumber = 1;
    private global::Budget.Data.Views.PageInfo pageInfo_;
    public global::Budget.Data.Views.PageInfo PageInfo {
      get { return pageInfo_; }
      set {
        pageInfo_ = value;
      }
    }

    /// <summary>Field number for the "filters" field.</summary>
    public const int FiltersFieldNumber = 2;
    private static readonly pb::FieldCodec<global::Budget.Data.Views.Filter> _repeated_filters_codec
        = pb::FieldCodec.ForMessage(18, global::Budget.Data.Views.Filter.Parser);
    private readonly pbc::RepeatedField<global::Budget.Data.Views.Filter> filters_ = new pbc::RepeatedField<global::Budget.Data.Views.Filter>();
    public pbc::RepeatedField<global::Budget.Data.Views.Filter> Filters {
      get { return filters_; }
    }

    /// <summary>Field number for the "transactions" field.</summary>
    public const int TransactionsFieldNumber = 3;
    private static readonly pb::FieldCodec<global::Budget.Data.Data.Transaction> _repeated_transactions_codec
        = pb::FieldCodec.ForMessage(26, global::Budget.Data.Data.Transaction.Parser);
    private readonly pbc::RepeatedField<global::Budget.Data.Data.Transaction> transactions_ = new pbc::RepeatedField<global::Budget.Data.Data.Transaction>();
    public pbc::RepeatedField<global::Budget.Data.Data.Transaction> Transactions {
      get { return transactions_; }
    }

    public override bool Equals(object other) {
      return Equals(other as TransactionList);
    }

    public bool Equals(TransactionList other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(PageInfo, other.PageInfo)) return false;
      if(!filters_.Equals(other.filters_)) return false;
      if(!transactions_.Equals(other.transactions_)) return false;
      return true;
    }

    public override int GetHashCode() {
      int hash = 1;
      if (pageInfo_ != null) hash ^= PageInfo.GetHashCode();
      hash ^= filters_.GetHashCode();
      hash ^= transactions_.GetHashCode();
      return hash;
    }

    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    public void WriteTo(pb::CodedOutputStream output) {
      if (pageInfo_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(PageInfo);
      }
      filters_.WriteTo(output, _repeated_filters_codec);
      transactions_.WriteTo(output, _repeated_transactions_codec);
    }

    public int CalculateSize() {
      int size = 0;
      if (pageInfo_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(PageInfo);
      }
      size += filters_.CalculateSize(_repeated_filters_codec);
      size += transactions_.CalculateSize(_repeated_transactions_codec);
      return size;
    }

    public void MergeFrom(TransactionList other) {
      if (other == null) {
        return;
      }
      if (other.pageInfo_ != null) {
        if (pageInfo_ == null) {
          pageInfo_ = new global::Budget.Data.Views.PageInfo();
        }
        PageInfo.MergeFrom(other.PageInfo);
      }
      filters_.Add(other.filters_);
      transactions_.Add(other.transactions_);
    }

    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            if (pageInfo_ == null) {
              pageInfo_ = new global::Budget.Data.Views.PageInfo();
            }
            input.ReadMessage(pageInfo_);
            break;
          }
          case 18: {
            filters_.AddEntriesFrom(input, _repeated_filters_codec);
            break;
          }
          case 26: {
            transactions_.AddEntriesFrom(input, _repeated_transactions_codec);
            break;
          }
        }
      }
    }

  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class UnsortedTransaction : pb::IMessage<UnsortedTransaction> {
    private static readonly pb::MessageParser<UnsortedTransaction> _parser = new pb::MessageParser<UnsortedTransaction>(() => new UnsortedTransaction());
    public static pb::MessageParser<UnsortedTransaction> Parser { get { return _parser; } }

    public static pbr::MessageDescriptor Descriptor {
      get { return global::Budget.Data.Views.ViewsReflection.Descriptor.MessageTypes[1]; }
    }

    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    public UnsortedTransaction() {
      OnConstruction();
    }

    partial void OnConstruction();

    public UnsortedTransaction(UnsortedTransaction other) : this() {
      Transaction = other.transaction_ != null ? other.Transaction.Clone() : null;
      categories_ = other.categories_.Clone();
    }

    public UnsortedTransaction Clone() {
      return new UnsortedTransaction(this);
    }

    /// <summary>Field number for the "transaction" field.</summary>
    public const int TransactionFieldNumber = 1;
    private global::Budget.Data.Data.Transaction transaction_;
    public global::Budget.Data.Data.Transaction Transaction {
      get { return transaction_; }
      set {
        transaction_ = value;
      }
    }

    /// <summary>Field number for the "categories" field.</summary>
    public const int CategoriesFieldNumber = 3;
    private static readonly pb::FieldCodec<global::Budget.Data.Data.TransactionCategory> _repeated_categories_codec
        = pb::FieldCodec.ForMessage(26, global::Budget.Data.Data.TransactionCategory.Parser);
    private readonly pbc::RepeatedField<global::Budget.Data.Data.TransactionCategory> categories_ = new pbc::RepeatedField<global::Budget.Data.Data.TransactionCategory>();
    public pbc::RepeatedField<global::Budget.Data.Data.TransactionCategory> Categories {
      get { return categories_; }
    }

    public override bool Equals(object other) {
      return Equals(other as UnsortedTransaction);
    }

    public bool Equals(UnsortedTransaction other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Transaction, other.Transaction)) return false;
      if(!categories_.Equals(other.categories_)) return false;
      return true;
    }

    public override int GetHashCode() {
      int hash = 1;
      if (transaction_ != null) hash ^= Transaction.GetHashCode();
      hash ^= categories_.GetHashCode();
      return hash;
    }

    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    public void WriteTo(pb::CodedOutputStream output) {
      if (transaction_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Transaction);
      }
      categories_.WriteTo(output, _repeated_categories_codec);
    }

    public int CalculateSize() {
      int size = 0;
      if (transaction_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Transaction);
      }
      size += categories_.CalculateSize(_repeated_categories_codec);
      return size;
    }

    public void MergeFrom(UnsortedTransaction other) {
      if (other == null) {
        return;
      }
      if (other.transaction_ != null) {
        if (transaction_ == null) {
          transaction_ = new global::Budget.Data.Data.Transaction();
        }
        Transaction.MergeFrom(other.Transaction);
      }
      categories_.Add(other.categories_);
    }

    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            if (transaction_ == null) {
              transaction_ = new global::Budget.Data.Data.Transaction();
            }
            input.ReadMessage(transaction_);
            break;
          }
          case 26: {
            categories_.AddEntriesFrom(input, _repeated_categories_codec);
            break;
          }
        }
      }
    }

  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class PageInfo : pb::IMessage<PageInfo> {
    private static readonly pb::MessageParser<PageInfo> _parser = new pb::MessageParser<PageInfo>(() => new PageInfo());
    public static pb::MessageParser<PageInfo> Parser { get { return _parser; } }

    public static pbr::MessageDescriptor Descriptor {
      get { return global::Budget.Data.Views.ViewsReflection.Descriptor.MessageTypes[2]; }
    }

    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    public PageInfo() {
      OnConstruction();
    }

    partial void OnConstruction();

    public PageInfo(PageInfo other) : this() {
      currentPage_ = other.currentPage_;
      pageCount_ = other.pageCount_;
      resultsPerPage_ = other.resultsPerPage_;
    }

    public PageInfo Clone() {
      return new PageInfo(this);
    }

    /// <summary>Field number for the "currentPage" field.</summary>
    public const int CurrentPageFieldNumber = 1;
    private uint currentPage_;
    public uint CurrentPage {
      get { return currentPage_; }
      set {
        currentPage_ = value;
      }
    }

    /// <summary>Field number for the "pageCount" field.</summary>
    public const int PageCountFieldNumber = 2;
    private uint pageCount_;
    public uint PageCount {
      get { return pageCount_; }
      set {
        pageCount_ = value;
      }
    }

    /// <summary>Field number for the "resultsPerPage" field.</summary>
    public const int ResultsPerPageFieldNumber = 3;
    private uint resultsPerPage_;
    public uint ResultsPerPage {
      get { return resultsPerPage_; }
      set {
        resultsPerPage_ = value;
      }
    }

    public override bool Equals(object other) {
      return Equals(other as PageInfo);
    }

    public bool Equals(PageInfo other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (CurrentPage != other.CurrentPage) return false;
      if (PageCount != other.PageCount) return false;
      if (ResultsPerPage != other.ResultsPerPage) return false;
      return true;
    }

    public override int GetHashCode() {
      int hash = 1;
      if (CurrentPage != 0) hash ^= CurrentPage.GetHashCode();
      if (PageCount != 0) hash ^= PageCount.GetHashCode();
      if (ResultsPerPage != 0) hash ^= ResultsPerPage.GetHashCode();
      return hash;
    }

    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    public void WriteTo(pb::CodedOutputStream output) {
      if (CurrentPage != 0) {
        output.WriteRawTag(8);
        output.WriteUInt32(CurrentPage);
      }
      if (PageCount != 0) {
        output.WriteRawTag(16);
        output.WriteUInt32(PageCount);
      }
      if (ResultsPerPage != 0) {
        output.WriteRawTag(24);
        output.WriteUInt32(ResultsPerPage);
      }
    }

    public int CalculateSize() {
      int size = 0;
      if (CurrentPage != 0) {
        size += 1 + pb::CodedOutputStream.ComputeUInt32Size(CurrentPage);
      }
      if (PageCount != 0) {
        size += 1 + pb::CodedOutputStream.ComputeUInt32Size(PageCount);
      }
      if (ResultsPerPage != 0) {
        size += 1 + pb::CodedOutputStream.ComputeUInt32Size(ResultsPerPage);
      }
      return size;
    }

    public void MergeFrom(PageInfo other) {
      if (other == null) {
        return;
      }
      if (other.CurrentPage != 0) {
        CurrentPage = other.CurrentPage;
      }
      if (other.PageCount != 0) {
        PageCount = other.PageCount;
      }
      if (other.ResultsPerPage != 0) {
        ResultsPerPage = other.ResultsPerPage;
      }
    }

    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 8: {
            CurrentPage = input.ReadUInt32();
            break;
          }
          case 16: {
            PageCount = input.ReadUInt32();
            break;
          }
          case 24: {
            ResultsPerPage = input.ReadUInt32();
            break;
          }
        }
      }
    }

  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class Filter : pb::IMessage<Filter> {
    private static readonly pb::MessageParser<Filter> _parser = new pb::MessageParser<Filter>(() => new Filter());
    public static pb::MessageParser<Filter> Parser { get { return _parser; } }

    public static pbr::MessageDescriptor Descriptor {
      get { return global::Budget.Data.Views.ViewsReflection.Descriptor.MessageTypes[3]; }
    }

    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    public Filter() {
      OnConstruction();
    }

    partial void OnConstruction();

    public Filter(Filter other) : this() {
      switch (other.FilterTypeCase) {
        case FilterTypeOneofCase.DateRangeFilter:
          DateRangeFilter = other.DateRangeFilter.Clone();
          break;
      }

    }

    public Filter Clone() {
      return new Filter(this);
    }

    /// <summary>Field number for the "dateRangeFilter" field.</summary>
    public const int DateRangeFilterFieldNumber = 1;
    public global::Budget.Data.Views.DateRangeFilter DateRangeFilter {
      get { return filterTypeCase_ == FilterTypeOneofCase.DateRangeFilter ? (global::Budget.Data.Views.DateRangeFilter) filterType_ : null; }
      set {
        filterType_ = value;
        filterTypeCase_ = value == null ? FilterTypeOneofCase.None : FilterTypeOneofCase.DateRangeFilter;
      }
    }

    private object filterType_;
    /// <summary>Enum of possible cases for the "filter_type" oneof.</summary>
    public enum FilterTypeOneofCase {
      None = 0,
      DateRangeFilter = 1,
    }
    private FilterTypeOneofCase filterTypeCase_ = FilterTypeOneofCase.None;
    public FilterTypeOneofCase FilterTypeCase {
      get { return filterTypeCase_; }
    }

    public void ClearFilterType() {
      filterTypeCase_ = FilterTypeOneofCase.None;
      filterType_ = null;
    }

    public override bool Equals(object other) {
      return Equals(other as Filter);
    }

    public bool Equals(Filter other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(DateRangeFilter, other.DateRangeFilter)) return false;
      if (FilterTypeCase != other.FilterTypeCase) return false;
      return true;
    }

    public override int GetHashCode() {
      int hash = 1;
      if (filterTypeCase_ == FilterTypeOneofCase.DateRangeFilter) hash ^= DateRangeFilter.GetHashCode();
      hash ^= (int) filterTypeCase_;
      return hash;
    }

    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    public void WriteTo(pb::CodedOutputStream output) {
      if (filterTypeCase_ == FilterTypeOneofCase.DateRangeFilter) {
        output.WriteRawTag(10);
        output.WriteMessage(DateRangeFilter);
      }
    }

    public int CalculateSize() {
      int size = 0;
      if (filterTypeCase_ == FilterTypeOneofCase.DateRangeFilter) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(DateRangeFilter);
      }
      return size;
    }

    public void MergeFrom(Filter other) {
      if (other == null) {
        return;
      }
      switch (other.FilterTypeCase) {
        case FilterTypeOneofCase.DateRangeFilter:
          DateRangeFilter = other.DateRangeFilter;
          break;
      }

    }

    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            global::Budget.Data.Views.DateRangeFilter subBuilder = new global::Budget.Data.Views.DateRangeFilter();
            if (filterTypeCase_ == FilterTypeOneofCase.DateRangeFilter) {
              subBuilder.MergeFrom(DateRangeFilter);
            }
            input.ReadMessage(subBuilder);
            DateRangeFilter = subBuilder;
            break;
          }
        }
      }
    }

  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class DateRangeFilter : pb::IMessage<DateRangeFilter> {
    private static readonly pb::MessageParser<DateRangeFilter> _parser = new pb::MessageParser<DateRangeFilter>(() => new DateRangeFilter());
    public static pb::MessageParser<DateRangeFilter> Parser { get { return _parser; } }

    public static pbr::MessageDescriptor Descriptor {
      get { return global::Budget.Data.Views.ViewsReflection.Descriptor.MessageTypes[4]; }
    }

    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    public DateRangeFilter() {
      OnConstruction();
    }

    partial void OnConstruction();

    public DateRangeFilter(DateRangeFilter other) : this() {
      start_ = other.start_;
      end_ = other.end_;
    }

    public DateRangeFilter Clone() {
      return new DateRangeFilter(this);
    }

    /// <summary>Field number for the "start" field.</summary>
    public const int StartFieldNumber = 1;
    private long start_;
    public long Start {
      get { return start_; }
      set {
        start_ = value;
      }
    }

    /// <summary>Field number for the "end" field.</summary>
    public const int EndFieldNumber = 2;
    private long end_;
    public long End {
      get { return end_; }
      set {
        end_ = value;
      }
    }

    public override bool Equals(object other) {
      return Equals(other as DateRangeFilter);
    }

    public bool Equals(DateRangeFilter other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Start != other.Start) return false;
      if (End != other.End) return false;
      return true;
    }

    public override int GetHashCode() {
      int hash = 1;
      if (Start != 0L) hash ^= Start.GetHashCode();
      if (End != 0L) hash ^= End.GetHashCode();
      return hash;
    }

    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    public void WriteTo(pb::CodedOutputStream output) {
      if (Start != 0L) {
        output.WriteRawTag(8);
        output.WriteInt64(Start);
      }
      if (End != 0L) {
        output.WriteRawTag(16);
        output.WriteInt64(End);
      }
    }

    public int CalculateSize() {
      int size = 0;
      if (Start != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(Start);
      }
      if (End != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(End);
      }
      return size;
    }

    public void MergeFrom(DateRangeFilter other) {
      if (other == null) {
        return;
      }
      if (other.Start != 0L) {
        Start = other.Start;
      }
      if (other.End != 0L) {
        End = other.End;
      }
    }

    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 8: {
            Start = input.ReadInt64();
            break;
          }
          case 16: {
            End = input.ReadInt64();
            break;
          }
        }
      }
    }

  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class ReportDataSeries : pb::IMessage<ReportDataSeries> {
    private static readonly pb::MessageParser<ReportDataSeries> _parser = new pb::MessageParser<ReportDataSeries>(() => new ReportDataSeries());
    public static pb::MessageParser<ReportDataSeries> Parser { get { return _parser; } }

    public static pbr::MessageDescriptor Descriptor {
      get { return global::Budget.Data.Views.ViewsReflection.Descriptor.MessageTypes[5]; }
    }

    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    public ReportDataSeries() {
      OnConstruction();
    }

    partial void OnConstruction();

    public ReportDataSeries(ReportDataSeries other) : this() {
      filters_ = other.filters_.Clone();
      frequency_ = other.frequency_;
      series_ = other.series_.Clone();
    }

    public ReportDataSeries Clone() {
      return new ReportDataSeries(this);
    }

    /// <summary>Field number for the "filters" field.</summary>
    public const int FiltersFieldNumber = 1;
    private static readonly pb::FieldCodec<global::Budget.Data.Views.Filter> _repeated_filters_codec
        = pb::FieldCodec.ForMessage(10, global::Budget.Data.Views.Filter.Parser);
    private readonly pbc::RepeatedField<global::Budget.Data.Views.Filter> filters_ = new pbc::RepeatedField<global::Budget.Data.Views.Filter>();
    public pbc::RepeatedField<global::Budget.Data.Views.Filter> Filters {
      get { return filters_; }
    }

    /// <summary>Field number for the "frequency" field.</summary>
    public const int FrequencyFieldNumber = 2;
    private long frequency_;
    public long Frequency {
      get { return frequency_; }
      set {
        frequency_ = value;
      }
    }

    /// <summary>Field number for the "series" field.</summary>
    public const int SeriesFieldNumber = 3;
    private static readonly pb::FieldCodec<global::Budget.Data.Views.ReportDataGroup> _repeated_series_codec
        = pb::FieldCodec.ForMessage(26, global::Budget.Data.Views.ReportDataGroup.Parser);
    private readonly pbc::RepeatedField<global::Budget.Data.Views.ReportDataGroup> series_ = new pbc::RepeatedField<global::Budget.Data.Views.ReportDataGroup>();
    public pbc::RepeatedField<global::Budget.Data.Views.ReportDataGroup> Series {
      get { return series_; }
    }

    public override bool Equals(object other) {
      return Equals(other as ReportDataSeries);
    }

    public bool Equals(ReportDataSeries other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!filters_.Equals(other.filters_)) return false;
      if (Frequency != other.Frequency) return false;
      if(!series_.Equals(other.series_)) return false;
      return true;
    }

    public override int GetHashCode() {
      int hash = 1;
      hash ^= filters_.GetHashCode();
      if (Frequency != 0L) hash ^= Frequency.GetHashCode();
      hash ^= series_.GetHashCode();
      return hash;
    }

    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    public void WriteTo(pb::CodedOutputStream output) {
      filters_.WriteTo(output, _repeated_filters_codec);
      if (Frequency != 0L) {
        output.WriteRawTag(16);
        output.WriteInt64(Frequency);
      }
      series_.WriteTo(output, _repeated_series_codec);
    }

    public int CalculateSize() {
      int size = 0;
      size += filters_.CalculateSize(_repeated_filters_codec);
      if (Frequency != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(Frequency);
      }
      size += series_.CalculateSize(_repeated_series_codec);
      return size;
    }

    public void MergeFrom(ReportDataSeries other) {
      if (other == null) {
        return;
      }
      filters_.Add(other.filters_);
      if (other.Frequency != 0L) {
        Frequency = other.Frequency;
      }
      series_.Add(other.series_);
    }

    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            filters_.AddEntriesFrom(input, _repeated_filters_codec);
            break;
          }
          case 16: {
            Frequency = input.ReadInt64();
            break;
          }
          case 26: {
            series_.AddEntriesFrom(input, _repeated_series_codec);
            break;
          }
        }
      }
    }

  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class ReportDataGroup : pb::IMessage<ReportDataGroup> {
    private static readonly pb::MessageParser<ReportDataGroup> _parser = new pb::MessageParser<ReportDataGroup>(() => new ReportDataGroup());
    public static pb::MessageParser<ReportDataGroup> Parser { get { return _parser; } }

    public static pbr::MessageDescriptor Descriptor {
      get { return global::Budget.Data.Views.ViewsReflection.Descriptor.MessageTypes[6]; }
    }

    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    public ReportDataGroup() {
      OnConstruction();
    }

    partial void OnConstruction();

    public ReportDataGroup(ReportDataGroup other) : this() {
      filters_ = other.filters_.Clone();
      data_ = other.data_.Clone();
    }

    public ReportDataGroup Clone() {
      return new ReportDataGroup(this);
    }

    /// <summary>Field number for the "filters" field.</summary>
    public const int FiltersFieldNumber = 1;
    private static readonly pb::FieldCodec<global::Budget.Data.Views.Filter> _repeated_filters_codec
        = pb::FieldCodec.ForMessage(10, global::Budget.Data.Views.Filter.Parser);
    private readonly pbc::RepeatedField<global::Budget.Data.Views.Filter> filters_ = new pbc::RepeatedField<global::Budget.Data.Views.Filter>();
    public pbc::RepeatedField<global::Budget.Data.Views.Filter> Filters {
      get { return filters_; }
    }

    /// <summary>Field number for the "data" field.</summary>
    public const int DataFieldNumber = 2;
    private static readonly pb::FieldCodec<global::Budget.Data.Views.ReportDataPoint> _repeated_data_codec
        = pb::FieldCodec.ForMessage(18, global::Budget.Data.Views.ReportDataPoint.Parser);
    private readonly pbc::RepeatedField<global::Budget.Data.Views.ReportDataPoint> data_ = new pbc::RepeatedField<global::Budget.Data.Views.ReportDataPoint>();
    public pbc::RepeatedField<global::Budget.Data.Views.ReportDataPoint> Data {
      get { return data_; }
    }

    public override bool Equals(object other) {
      return Equals(other as ReportDataGroup);
    }

    public bool Equals(ReportDataGroup other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!filters_.Equals(other.filters_)) return false;
      if(!data_.Equals(other.data_)) return false;
      return true;
    }

    public override int GetHashCode() {
      int hash = 1;
      hash ^= filters_.GetHashCode();
      hash ^= data_.GetHashCode();
      return hash;
    }

    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    public void WriteTo(pb::CodedOutputStream output) {
      filters_.WriteTo(output, _repeated_filters_codec);
      data_.WriteTo(output, _repeated_data_codec);
    }

    public int CalculateSize() {
      int size = 0;
      size += filters_.CalculateSize(_repeated_filters_codec);
      size += data_.CalculateSize(_repeated_data_codec);
      return size;
    }

    public void MergeFrom(ReportDataGroup other) {
      if (other == null) {
        return;
      }
      filters_.Add(other.filters_);
      data_.Add(other.data_);
    }

    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            filters_.AddEntriesFrom(input, _repeated_filters_codec);
            break;
          }
          case 18: {
            data_.AddEntriesFrom(input, _repeated_data_codec);
            break;
          }
        }
      }
    }

  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class ReportDataPoint : pb::IMessage<ReportDataPoint> {
    private static readonly pb::MessageParser<ReportDataPoint> _parser = new pb::MessageParser<ReportDataPoint>(() => new ReportDataPoint());
    public static pb::MessageParser<ReportDataPoint> Parser { get { return _parser; } }

    public static pbr::MessageDescriptor Descriptor {
      get { return global::Budget.Data.Views.ViewsReflection.Descriptor.MessageTypes[7]; }
    }

    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    public ReportDataPoint() {
      OnConstruction();
    }

    partial void OnConstruction();

    public ReportDataPoint(ReportDataPoint other) : this() {
      Category = other.category_ != null ? other.Category.Clone() : null;
      cents_ = other.cents_;
    }

    public ReportDataPoint Clone() {
      return new ReportDataPoint(this);
    }

    /// <summary>Field number for the "category" field.</summary>
    public const int CategoryFieldNumber = 1;
    private global::Budget.Data.Data.TransactionCategory category_;
    public global::Budget.Data.Data.TransactionCategory Category {
      get { return category_; }
      set {
        category_ = value;
      }
    }

    /// <summary>Field number for the "cents" field.</summary>
    public const int CentsFieldNumber = 2;
    private int cents_;
    public int Cents {
      get { return cents_; }
      set {
        cents_ = value;
      }
    }

    public override bool Equals(object other) {
      return Equals(other as ReportDataPoint);
    }

    public bool Equals(ReportDataPoint other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Category, other.Category)) return false;
      if (Cents != other.Cents) return false;
      return true;
    }

    public override int GetHashCode() {
      int hash = 1;
      if (category_ != null) hash ^= Category.GetHashCode();
      if (Cents != 0) hash ^= Cents.GetHashCode();
      return hash;
    }

    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    public void WriteTo(pb::CodedOutputStream output) {
      if (category_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Category);
      }
      if (Cents != 0) {
        output.WriteRawTag(16);
        output.WriteSInt32(Cents);
      }
    }

    public int CalculateSize() {
      int size = 0;
      if (category_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Category);
      }
      if (Cents != 0) {
        size += 1 + pb::CodedOutputStream.ComputeSInt32Size(Cents);
      }
      return size;
    }

    public void MergeFrom(ReportDataPoint other) {
      if (other == null) {
        return;
      }
      if (other.category_ != null) {
        if (category_ == null) {
          category_ = new global::Budget.Data.Data.TransactionCategory();
        }
        Category.MergeFrom(other.Category);
      }
      if (other.Cents != 0) {
        Cents = other.Cents;
      }
    }

    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            if (category_ == null) {
              category_ = new global::Budget.Data.Data.TransactionCategory();
            }
            input.ReadMessage(category_);
            break;
          }
          case 16: {
            Cents = input.ReadSInt32();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
