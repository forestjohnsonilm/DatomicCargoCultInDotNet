// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: Test.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace DatomicNet.Core.Tests {

  /// <summary>Holder for reflection information generated from Test.proto</summary>
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public static partial class TestReflection {

    #region Descriptor
    /// <summary>File descriptor for Test.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static TestReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CgpUZXN0LnByb3RvEhVEYXRvbWljTmV0LkNvcmUuVGVzdHMipwEKDVRlc3RB",
            "Z2dyZWdhdGUSCgoCaWQYASABKAQSPgoKY2F0ZWdvcmllcxgDIAMoCzIqLkRh",
            "dG9taWNOZXQuQ29yZS5UZXN0cy5UcmFuc2FjdGlvbkNhdGVnb3J5EhMKC2Rl",
            "c2NyaXB0aW9uGAcgASgJEjUKCm90aGVyVGhpbmcYCSABKAsyIS5EYXRvbWlj",
            "TmV0LkNvcmUuVGVzdHMuT3RoZXJUaGluZyJAChNUcmFuc2FjdGlvbkNhdGVn",
            "b3J5EgoKAmlkGAEgASgEEg8KB2lzR3JlYXQYAiABKAgSDAoEbmFtZRgDIAEo",
            "CSI5CgpPdGhlclRoaW5nEgoKAmlkGAEgASgEEgkKAWgYAiABKAISCQoBcxgD",
            "IAEoAhIJCgF2GAQgASgCQhiqAhVEYXRvbWljTmV0LkNvcmUuVGVzdHNiBnBy",
            "b3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedCodeInfo(null, new pbr::GeneratedCodeInfo[] {
            new pbr::GeneratedCodeInfo(typeof(global::DatomicNet.Core.Tests.TestAggregate), global::DatomicNet.Core.Tests.TestAggregate.Parser, new[]{ "Id", "Categories", "Description", "OtherThing" }, null, null, null),
            new pbr::GeneratedCodeInfo(typeof(global::DatomicNet.Core.Tests.TransactionCategory), global::DatomicNet.Core.Tests.TransactionCategory.Parser, new[]{ "Id", "IsGreat", "Name" }, null, null, null),
            new pbr::GeneratedCodeInfo(typeof(global::DatomicNet.Core.Tests.OtherThing), global::DatomicNet.Core.Tests.OtherThing.Parser, new[]{ "Id", "H", "S", "V" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class TestAggregate : pb::IMessage<TestAggregate> {
    private static readonly pb::MessageParser<TestAggregate> _parser = new pb::MessageParser<TestAggregate>(() => new TestAggregate());
    public static pb::MessageParser<TestAggregate> Parser { get { return _parser; } }

    public static pbr::MessageDescriptor Descriptor {
      get { return global::DatomicNet.Core.Tests.TestReflection.Descriptor.MessageTypes[0]; }
    }

    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    public TestAggregate() {
      OnConstruction();
    }

    partial void OnConstruction();

    public TestAggregate(TestAggregate other) : this() {
      id_ = other.id_;
      categories_ = other.categories_.Clone();
      description_ = other.description_;
      OtherThing = other.otherThing_ != null ? other.OtherThing.Clone() : null;
    }

    public TestAggregate Clone() {
      return new TestAggregate(this);
    }

    /// <summary>Field number for the "id" field.</summary>
    public const int IdFieldNumber = 1;
    private ulong id_;
    public ulong Id {
      get { return id_; }
      set {
        id_ = value;
      }
    }

    /// <summary>Field number for the "categories" field.</summary>
    public const int CategoriesFieldNumber = 3;
    private static readonly pb::FieldCodec<global::DatomicNet.Core.Tests.TransactionCategory> _repeated_categories_codec
        = pb::FieldCodec.ForMessage(26, global::DatomicNet.Core.Tests.TransactionCategory.Parser);
    private readonly pbc::RepeatedField<global::DatomicNet.Core.Tests.TransactionCategory> categories_ = new pbc::RepeatedField<global::DatomicNet.Core.Tests.TransactionCategory>();
    public pbc::RepeatedField<global::DatomicNet.Core.Tests.TransactionCategory> Categories {
      get { return categories_; }
    }

    /// <summary>Field number for the "description" field.</summary>
    public const int DescriptionFieldNumber = 7;
    private string description_ = "";
    public string Description {
      get { return description_; }
      set {
        description_ = pb::Preconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "otherThing" field.</summary>
    public const int OtherThingFieldNumber = 9;
    private global::DatomicNet.Core.Tests.OtherThing otherThing_;
    public global::DatomicNet.Core.Tests.OtherThing OtherThing {
      get { return otherThing_; }
      set {
        otherThing_ = value;
      }
    }

    public override bool Equals(object other) {
      return Equals(other as TestAggregate);
    }

    public bool Equals(TestAggregate other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Id != other.Id) return false;
      if(!categories_.Equals(other.categories_)) return false;
      if (Description != other.Description) return false;
      if (!object.Equals(OtherThing, other.OtherThing)) return false;
      return true;
    }

    public override int GetHashCode() {
      int hash = 1;
      if (Id != 0UL) hash ^= Id.GetHashCode();
      hash ^= categories_.GetHashCode();
      if (Description.Length != 0) hash ^= Description.GetHashCode();
      if (otherThing_ != null) hash ^= OtherThing.GetHashCode();
      return hash;
    }

    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    public void WriteTo(pb::CodedOutputStream output) {
      if (Id != 0UL) {
        output.WriteRawTag(8);
        output.WriteUInt64(Id);
      }
      categories_.WriteTo(output, _repeated_categories_codec);
      if (Description.Length != 0) {
        output.WriteRawTag(58);
        output.WriteString(Description);
      }
      if (otherThing_ != null) {
        output.WriteRawTag(74);
        output.WriteMessage(OtherThing);
      }
    }

    public int CalculateSize() {
      int size = 0;
      if (Id != 0UL) {
        size += 1 + pb::CodedOutputStream.ComputeUInt64Size(Id);
      }
      size += categories_.CalculateSize(_repeated_categories_codec);
      if (Description.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Description);
      }
      if (otherThing_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(OtherThing);
      }
      return size;
    }

    public void MergeFrom(TestAggregate other) {
      if (other == null) {
        return;
      }
      if (other.Id != 0UL) {
        Id = other.Id;
      }
      categories_.Add(other.categories_);
      if (other.Description.Length != 0) {
        Description = other.Description;
      }
      if (other.otherThing_ != null) {
        if (otherThing_ == null) {
          otherThing_ = new global::DatomicNet.Core.Tests.OtherThing();
        }
        OtherThing.MergeFrom(other.OtherThing);
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
            Id = input.ReadUInt64();
            break;
          }
          case 26: {
            categories_.AddEntriesFrom(input, _repeated_categories_codec);
            break;
          }
          case 58: {
            Description = input.ReadString();
            break;
          }
          case 74: {
            if (otherThing_ == null) {
              otherThing_ = new global::DatomicNet.Core.Tests.OtherThing();
            }
            input.ReadMessage(otherThing_);
            break;
          }
        }
      }
    }

  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class TransactionCategory : pb::IMessage<TransactionCategory> {
    private static readonly pb::MessageParser<TransactionCategory> _parser = new pb::MessageParser<TransactionCategory>(() => new TransactionCategory());
    public static pb::MessageParser<TransactionCategory> Parser { get { return _parser; } }

    public static pbr::MessageDescriptor Descriptor {
      get { return global::DatomicNet.Core.Tests.TestReflection.Descriptor.MessageTypes[1]; }
    }

    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    public TransactionCategory() {
      OnConstruction();
    }

    partial void OnConstruction();

    public TransactionCategory(TransactionCategory other) : this() {
      id_ = other.id_;
      isGreat_ = other.isGreat_;
      name_ = other.name_;
    }

    public TransactionCategory Clone() {
      return new TransactionCategory(this);
    }

    /// <summary>Field number for the "id" field.</summary>
    public const int IdFieldNumber = 1;
    private ulong id_;
    public ulong Id {
      get { return id_; }
      set {
        id_ = value;
      }
    }

    /// <summary>Field number for the "isGreat" field.</summary>
    public const int IsGreatFieldNumber = 2;
    private bool isGreat_;
    public bool IsGreat {
      get { return isGreat_; }
      set {
        isGreat_ = value;
      }
    }

    /// <summary>Field number for the "name" field.</summary>
    public const int NameFieldNumber = 3;
    private string name_ = "";
    public string Name {
      get { return name_; }
      set {
        name_ = pb::Preconditions.CheckNotNull(value, "value");
      }
    }

    public override bool Equals(object other) {
      return Equals(other as TransactionCategory);
    }

    public bool Equals(TransactionCategory other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Id != other.Id) return false;
      if (IsGreat != other.IsGreat) return false;
      if (Name != other.Name) return false;
      return true;
    }

    public override int GetHashCode() {
      int hash = 1;
      if (Id != 0UL) hash ^= Id.GetHashCode();
      if (IsGreat != false) hash ^= IsGreat.GetHashCode();
      if (Name.Length != 0) hash ^= Name.GetHashCode();
      return hash;
    }

    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    public void WriteTo(pb::CodedOutputStream output) {
      if (Id != 0UL) {
        output.WriteRawTag(8);
        output.WriteUInt64(Id);
      }
      if (IsGreat != false) {
        output.WriteRawTag(16);
        output.WriteBool(IsGreat);
      }
      if (Name.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(Name);
      }
    }

    public int CalculateSize() {
      int size = 0;
      if (Id != 0UL) {
        size += 1 + pb::CodedOutputStream.ComputeUInt64Size(Id);
      }
      if (IsGreat != false) {
        size += 1 + 1;
      }
      if (Name.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      return size;
    }

    public void MergeFrom(TransactionCategory other) {
      if (other == null) {
        return;
      }
      if (other.Id != 0UL) {
        Id = other.Id;
      }
      if (other.IsGreat != false) {
        IsGreat = other.IsGreat;
      }
      if (other.Name.Length != 0) {
        Name = other.Name;
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
            Id = input.ReadUInt64();
            break;
          }
          case 16: {
            IsGreat = input.ReadBool();
            break;
          }
          case 26: {
            Name = input.ReadString();
            break;
          }
        }
      }
    }

  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class OtherThing : pb::IMessage<OtherThing> {
    private static readonly pb::MessageParser<OtherThing> _parser = new pb::MessageParser<OtherThing>(() => new OtherThing());
    public static pb::MessageParser<OtherThing> Parser { get { return _parser; } }

    public static pbr::MessageDescriptor Descriptor {
      get { return global::DatomicNet.Core.Tests.TestReflection.Descriptor.MessageTypes[2]; }
    }

    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    public OtherThing() {
      OnConstruction();
    }

    partial void OnConstruction();

    public OtherThing(OtherThing other) : this() {
      id_ = other.id_;
      h_ = other.h_;
      s_ = other.s_;
      v_ = other.v_;
    }

    public OtherThing Clone() {
      return new OtherThing(this);
    }

    /// <summary>Field number for the "id" field.</summary>
    public const int IdFieldNumber = 1;
    private ulong id_;
    public ulong Id {
      get { return id_; }
      set {
        id_ = value;
      }
    }

    /// <summary>Field number for the "h" field.</summary>
    public const int HFieldNumber = 2;
    private float h_;
    public float H {
      get { return h_; }
      set {
        h_ = value;
      }
    }

    /// <summary>Field number for the "s" field.</summary>
    public const int SFieldNumber = 3;
    private float s_;
    public float S {
      get { return s_; }
      set {
        s_ = value;
      }
    }

    /// <summary>Field number for the "v" field.</summary>
    public const int VFieldNumber = 4;
    private float v_;
    public float V {
      get { return v_; }
      set {
        v_ = value;
      }
    }

    public override bool Equals(object other) {
      return Equals(other as OtherThing);
    }

    public bool Equals(OtherThing other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Id != other.Id) return false;
      if (H != other.H) return false;
      if (S != other.S) return false;
      if (V != other.V) return false;
      return true;
    }

    public override int GetHashCode() {
      int hash = 1;
      if (Id != 0UL) hash ^= Id.GetHashCode();
      if (H != 0F) hash ^= H.GetHashCode();
      if (S != 0F) hash ^= S.GetHashCode();
      if (V != 0F) hash ^= V.GetHashCode();
      return hash;
    }

    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    public void WriteTo(pb::CodedOutputStream output) {
      if (Id != 0UL) {
        output.WriteRawTag(8);
        output.WriteUInt64(Id);
      }
      if (H != 0F) {
        output.WriteRawTag(21);
        output.WriteFloat(H);
      }
      if (S != 0F) {
        output.WriteRawTag(29);
        output.WriteFloat(S);
      }
      if (V != 0F) {
        output.WriteRawTag(37);
        output.WriteFloat(V);
      }
    }

    public int CalculateSize() {
      int size = 0;
      if (Id != 0UL) {
        size += 1 + pb::CodedOutputStream.ComputeUInt64Size(Id);
      }
      if (H != 0F) {
        size += 1 + 4;
      }
      if (S != 0F) {
        size += 1 + 4;
      }
      if (V != 0F) {
        size += 1 + 4;
      }
      return size;
    }

    public void MergeFrom(OtherThing other) {
      if (other == null) {
        return;
      }
      if (other.Id != 0UL) {
        Id = other.Id;
      }
      if (other.H != 0F) {
        H = other.H;
      }
      if (other.S != 0F) {
        S = other.S;
      }
      if (other.V != 0F) {
        V = other.V;
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
            Id = input.ReadUInt64();
            break;
          }
          case 21: {
            H = input.ReadFloat();
            break;
          }
          case 29: {
            S = input.ReadFloat();
            break;
          }
          case 37: {
            V = input.ReadFloat();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
