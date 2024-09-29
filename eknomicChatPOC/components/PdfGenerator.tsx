import React from 'react';
import { View, Button, StyleSheet, Alert } from 'react-native';
import RNHTMLtoPDF from 'react-native-html-to-pdf';
import * as Print from 'expo-print';
import { shareAsync } from 'expo-sharing';
interface PdfGeneratorProps {
  fields: { title: string; value: string }[];
}

const PdfGenerator: React.FC<PdfGeneratorProps> = ({ fields }) => {
    console.log('fields', fields)
    const html = `
    <h1>Added Fields</h1>
    <ul>
      ${fields.map(field => `<li>${field.title}: ${field.value}</li>`).join('')}
    </ul>
  `;
    const printToFile = async () => {
      // On iOS/android prints the given html. On web prints the HTML from the current page.
      const { uri } = await Print.printToFileAsync({ html });
      console.log('File has been saved to:', uri);
      await shareAsync(uri, { UTI: '.pdf', mimeType: 'application/pdf' });
    };
  const createPDF = async () => {
    const html = `
      <h1>Added Fields</h1>
      <ul>
        ${fields.map(field => `<li>${field.title}: ${field.value}</li>`).join('')}
      </ul>
    `;

    const options = {
      html,
      fileName: 'AddedFields',
      directory: 'Documents',
    };
    console.log('options', options)

    try {
      const file = await RNHTMLtoPDF.convert(options);
      console.log(file.filePath); // Path to the generated PDF
      Alert.alert('PDF generated', 'PDF generated: ' + file.filePath);
    } catch (error) {
      console.error(error);
      Alert.alert('Error', 'Failed to generate PDF');
    }
  };

  return (
    <View style={styles.container}>
      <Button title="Download PDF" onPress={printToFile} />
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    marginTop: 20,
  },
});

export default PdfGenerator;
