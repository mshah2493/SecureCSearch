using O2S.Components.PDF4NET.Graphics;
using O2S.Components.PDF4NET.PageObjects;
using O2S.Components.PDF4NET.Text;
using securePDFmerging.Models;
using System;
using System.Collections.Generic;

namespace securePDFmerging
{
    class PDFObjectsProcessor
    {
        /**
         * Processes PDFObjects and returns words
         * 
         */
        public List<Words> GetWords(PDFTextPageObject TextObject, List<Words> WordObjects)
        {
            Words Word;
            bool firstCharacter;
            FontsProcessor fontsProcessor = new FontsProcessor();
            PDFGlyphCollection GlyphCollection = TextObject.Glyphs;

            if (WordObjects.Count == 0 || WordObjects[WordObjects.Count - 1].WordComplete)
            {
                firstCharacter = true;
                Word = new Words();
            }
            else
            {
                firstCharacter = false;
                Word = WordObjects[WordObjects.Count - 1];
                Word.Added = false;
                WordObjects.RemoveAt(WordObjects.Count - 1);
            }

            for (int i = 0; i < GlyphCollection.Count; i++)
            {
                if (String.IsNullOrWhiteSpace(GlyphCollection[i].Text))
                {
                    firstCharacter = true;

                    if (!String.IsNullOrEmpty(Word.Word))
                    {
                        Word.WordComplete = true;

                        if (!Word.Added)
                        {
                            Word.Added = true;
                            WordObjects.Add(Word);
                        }

                        Word = new Words();
                    }
                }
                else
                {
                    Word.Word = Word.Word + GlyphCollection[i].Text;

                    if (firstCharacter)
                    {
                        firstCharacter = false;

                        Word.FontBase = fontsProcessor.GetFontBase(TextObject.FontName, TextObject.FontSize);
                        Word.FontBrush = new PDFBrush(TextObject.FillColor);
                        Word.Top = GlyphCollection[i].DisplayBounds.Top;
                        Word.Left = GlyphCollection[i].DisplayBounds.Left;
                    }
                }
            }

            if (Word.Word != "")
            {
                Word.Added = true;
                WordObjects.Add(Word);
            }

            return WordObjects;
        }

        /**
         * Processes PDFObjects and returns blocks
         * 
         */
        public List<Blocks> GetBlocks(PDFTextPageObject TextObject, List<Blocks> BlockList)
        {
            Blocks block;
            bool firstCharacter;
            FontsProcessor fontsProcessor = new FontsProcessor();
            PDFGlyphCollection GlyphCollection = TextObject.Glyphs;

            if (BlockList.Count == 0 || BlockList[BlockList.Count - 1].BlockComplete)
            {
                firstCharacter = true;
                block = new Blocks();
            }
            else
            {
                firstCharacter = false;
                block = BlockList[BlockList.Count - 1];
                BlockList.RemoveAt(BlockList.Count - 1);
            }

            for (int i = 0; i < GlyphCollection.Count; i++)
            {
                block.Block = block.Block + GlyphCollection[i].Text;

                if (block.Block.Length == 16)
                {
                    block.BlockComplete = true;
                    BlockList.Add(block);

                    if (++i >= GlyphCollection.Count)
                    {
                        return BlockList; 
                    }

                    firstCharacter = true;
                    block = new Blocks();
                    block.Block = block.Block + GlyphCollection[i].Text;
                }

                if (firstCharacter)
                {
                    firstCharacter = false;

                    block.FontBase = fontsProcessor.GetFontBase(TextObject.FontName, TextObject.FontSize);
                    block.FontBrush = new PDFBrush(TextObject.FillColor);
                    block.Top.Add(GlyphCollection[i].DisplayBounds.Top);
                    block.Left.Add(GlyphCollection[i].DisplayBounds.Left);
                } 
                else
                {
                    if (GlyphCollection[i].DisplayBounds.Top > block.TopOfLastCharacter)
                    {
                        block.Chunks.Add(block.Block.Length - 1);
                        block.Top.Add(GlyphCollection[i].DisplayBounds.Top);
                        block.Left.Add(GlyphCollection[i].DisplayBounds.Left);
                    }
                }

                block.TopOfLastCharacter = GlyphCollection[i].DisplayBounds.Top;
            }

            BlockList.Add(block);

            return BlockList;
        }
    }
}
